﻿namespace TomsToolbox.Core
{
    using System;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Common interface for weak event listener.
    /// </summary>
    public interface IWeakEventListener
    {
        /// <summary>
        /// Detaches from the subscribed event.
        /// </summary>
        void Detach();
    }

    /// <summary>
    /// Implements a weak event listener that allows the owner to be garbage
    /// collected if its only remaining link is an event handler.
    /// </summary>
    /// <typeparam name="TTarget">Type of the target instance listening for the event.</typeparam>
    /// <typeparam name="TSource">Type of source instance for the event.</typeparam>
    /// <typeparam name="TEventArgs">Type of event arguments for the event.</typeparam>
    public class WeakEventListener<TTarget, TSource, TEventArgs> : IWeakEventListener
        where TTarget : class
        where TSource : class
    {
        /// <summary>
        /// WeakReference to the object listening for the event.
        /// </summary>
        private readonly WeakReference<TTarget> _weakTarget;

        /// <summary>
        /// To hold only a reference to source object. With this instance the WeakEventListener
        /// can guarantee that the handler gets unregistered when listener is released but does not reference the source.
        /// </summary>
        private readonly WeakReference<TSource> _weakSource;
        /// <summary>
        /// To hold a reference to source object. With this instance the WeakEventListener
        /// can guarantee that the handler gets unregistered when listener is released.
        /// </summary>
        private readonly TSource _source;

        /// <summary>
        /// Delegate to the method to call when the event fires.
        /// </summary>
        private readonly Action<TTarget, object, TEventArgs> _onEventAction;

        /// <summary>
        /// Delegate to the method to call when detaching from the event.
        /// </summary>
        private readonly Action<WeakEventListener<TTarget, TSource, TEventArgs>, TSource> _onDetachAction;

        /// <summary>
        /// Initializes a new instances of the WeakEventListener class that references the source but not the target.
        /// </summary>
        /// <param name="target">Instance subscribing to the event. The instance will not be referenced.</param>
        /// <param name="source">Instance providing the event. The instance will be referenced.</param>
        /// <param name="onEventAction">The static method to call when a event is received.</param>
        /// <param name="onAttachAction">The static action to attach to the event(s).</param>
        /// <param name="onDetachAction">The static action to detach from the event(s).</param>
        public WeakEventListener(TTarget target, TSource source,
            Action<TTarget, object, TEventArgs> onEventAction,
            Action<WeakEventListener<TTarget, TSource, TEventArgs>, TSource> onAttachAction,
            Action<WeakEventListener<TTarget, TSource, TEventArgs>, TSource> onDetachAction)
        {
            Contract.Requires(target != null);
            Contract.Requires(source != null);
            Contract.Requires(onEventAction != null);
            Contract.Requires(onAttachAction != null);
            Contract.Requires(onDetachAction != null);
            Contract.Requires(onEventAction.Method.IsStatic, "Method must be static, otherwise the event WeakEventListner class does not prevent memory leaks.");
            Contract.Requires(onAttachAction.Method.IsStatic, "Method must be static, otherwise the event WeakEventListner class does not prevent memory leaks.");
            Contract.Requires(onDetachAction.Method.IsStatic, "Method must be static, otherwise the event WeakEventListner class does not prevent memory leaks.");

            _weakTarget = new WeakReference<TTarget>(target);
            _source = source;
            _onEventAction = onEventAction;
            _onDetachAction = onDetachAction;

            onAttachAction(this, source);
        }

        /// <summary>
        /// Initializes a new instances of the WeakEventListener class that does not reference both source and target.
        /// </summary>
        /// <param name="target">Instance subscribing to the event. The instance will not be referenced.</param>
        /// <param name="source">Weak reference to the instance providing the event. When using this constructor the source will not be referenced, too.</param>
        /// <param name="onEventAction">The static method to call when a event is received.</param>
        /// <param name="onAttachAction">The static action to attach to the event(s).</param>
        /// <param name="onDetachAction">The static action to detach from the event(s).</param>
        public WeakEventListener(TTarget target, WeakReference<TSource> source,
            Action<TTarget, object, TEventArgs> onEventAction,
            Action<WeakEventListener<TTarget, TSource, TEventArgs>, TSource> onAttachAction,
            Action<WeakEventListener<TTarget, TSource, TEventArgs>, TSource> onDetachAction)
        {
            Contract.Requires(target != null);
            Contract.Requires(source != null);
            Contract.Requires(onEventAction != null);
            Contract.Requires(onAttachAction != null);
            Contract.Requires(onDetachAction != null);
            Contract.Requires(onEventAction.Method.IsStatic, "Method must be static, otherwise the event WeakEventListner class does not prevent memory leaks.");
            Contract.Requires(onAttachAction.Method.IsStatic, "Method must be static, otherwise the event WeakEventListner class does not prevent memory leaks.");
            Contract.Requires(onDetachAction.Method.IsStatic, "Method must be static, otherwise the event WeakEventListner class does not prevent memory leaks.");

            _weakTarget = new WeakReference<TTarget>(target);
            _weakSource = source;
            _onEventAction = onEventAction;
            _onDetachAction = onDetachAction;

            onAttachAction(this, source.Target);
        }

        /// <summary>
        /// Handler for the subscribed event calls OnEventAction to handle it.
        /// </summary>
        /// <param name="source">Event source.</param>
        /// <param name="eventArgs">Event arguments.</param>
        public void OnEvent(object source, TEventArgs eventArgs)
        {
            TTarget target;

            if (_weakTarget.TryGetTarget(out target))
            {
                // Call registered action
                _onEventAction(target, source, eventArgs);
            }
            else
            {
                // Detach from event
                Detach();
            }
        }

        /// <summary>
        /// Detaches from the subscribed event.
        /// </summary>
        public void Detach()
        {
            var source = _source ?? _weakSource.Target;
            if (source == null)
                return;

            _onDetachAction(this, source);
        }

        [ContractInvariantMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Required for code contracts.")]
        private void ObjectInvariant()
        {
            Contract.Invariant(_weakTarget != null);
            Contract.Invariant((_source != null) || (_weakSource != null));
            Contract.Invariant(_onEventAction != null);
            Contract.Invariant(_onDetachAction != null);
        }
    }
}