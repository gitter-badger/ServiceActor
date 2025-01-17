﻿using Nito.AsyncEx;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace ServiceActor
{
    public partial class ActionQueue
    {
        private readonly ActionBlock<InvocationItem> _actionQueue;
        private int? _executingActionThreadId;
        private InvocationItem _executingInvocationItem;

        private static int _callTimeout = 30000;
        public static int CallTimeout
        {
            get => _callTimeout;
            set
            {
                if (value != Timeout.Infinite && value < 0)
                {
                    throw new ArgumentException("Invalid value");
                }

                _callTimeout = value == 0 ? Timeout.Infinite : value;
            }
        }

        public ActionQueue()
        {
            _actionQueue = new ActionBlock<InvocationItem>(invocation =>
            {
                if (invocation.KeepContextForAsyncCalls)
                {
                    //Console.WriteLine($"Current Thread ID Before action.Invoke: {Thread.CurrentThread.ManagedThreadId}");
                    _executingActionThreadId = Thread.CurrentThread.ManagedThreadId;

                    try
                    {
                        //System.Diagnostics.Debug.WriteLine($"-----Executing {invocation.Target?.WrappedObject}({invocation.TypeOfObjectToWrap}) {invocation.Action.Method}...");
                        if (_actionCallMonitor != null)
                        {
                            var callDetails = new CallDetails(this, invocation.Target, invocation.Target?.WrappedObject, invocation.TypeOfObjectToWrap, invocation.Action);
                            _actionCallMonitor?.EnterMethod(callDetails);
                        }
                        _executingInvocationItem = invocation;
                        AsyncContext.Run(invocation.Action);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }


                    //System.Diagnostics.Debug.WriteLine($"-----Executed {invocation.Target?.WrappedObject}({invocation.TypeOfObjectToWrap}) {invocation.Action.Method}");
                    if (_actionCallMonitor != null)
                    {
                        var callDetails = new CallDetails(this, invocation.Target, invocation.Target?.WrappedObject, invocation.TypeOfObjectToWrap, invocation.Action);
                        _actionCallMonitor?.ExitMethod(callDetails);
                    }

                    var executingInvocationItem = _executingInvocationItem;
                    _executingInvocationItem = null;
                    _executingActionThreadId = null;

                    executingInvocationItem.SignalExecuted();
                    //action.Invoke();
                    //Console.WriteLine($"Current Thread ID After action.Invoke: {Thread.CurrentThread.ManagedThreadId}");
                }
                else
                {
                    invocation.Action();
                }
            });
        }

        public void Stop()
        {
            _actionQueue.Complete();
        }

        public InvocationItem Enqueue(IServiceActorWrapper target, string typeOfObjectToWrap, Action action, bool keepContextForAsyncCalls = true, bool asyncEvent = false)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (typeOfObjectToWrap == null)
            {
                throw new ArgumentNullException(nameof(typeOfObjectToWrap));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (Thread.CurrentThread.ManagedThreadId == _executingActionThreadId)
            {
                //if the calling thread is the same as the executing action thread then just pass thru
                action();
                return null;
            }

            var invocationItem = new InvocationItem(
                action, 
                target,
                typeOfObjectToWrap,
                keepContextForAsyncCalls,
                asyncEvent);

            _actionQueue.Post(invocationItem);

            return invocationItem;
        }

        public InvocationItem Enqueue(Action action, bool keepContextForAsyncCalls = true, bool asyncEvent = false)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (Thread.CurrentThread.ManagedThreadId == _executingActionThreadId)
            {
                //if the calling thread is the same as the first executing action then just pass thru
                action();
                return _executingInvocationItem;
            }

            var invocationItem = new InvocationItem(
                action,
                keepContextForAsyncCalls,
                asyncEvent
            );

            _actionQueue.Post(invocationItem);

            return invocationItem;
        }

        #region Calls Monitor
        private static IActionCallMonitor _actionCallMonitor;
        public static void BeginMonitor(IActionCallMonitor actionCallMonitor)
        {
            _actionCallMonitor = actionCallMonitor ?? throw new ArgumentNullException(nameof(actionCallMonitor));
        }

        public static void ExitMonitor(IActionCallMonitor actionCallMonitor)
        {
            if (actionCallMonitor == null)
            {
                throw new ArgumentNullException(nameof(actionCallMonitor));
            }

            if (actionCallMonitor != _actionCallMonitor)
            {
                throw new InvalidOperationException();
            }

            _actionCallMonitor = null;
        }
        #endregion

        #region Pending Operations
        public void RegisterPendingOperation(IPendingOperation pendingOperation)
        {
            if (pendingOperation == null)
            {
                throw new ArgumentNullException(nameof(pendingOperation));
            }

            _executingInvocationItem.EnqueuePendingOperation(pendingOperation);
        }

        public void RegisterPendingOperation(WaitHandle waitHandle, int timeoutMilliseconds = 0, Action<bool> actionOnCompletion = null)
        {
            RegisterPendingOperation(new WaitHandlerPendingOperation(waitHandle, timeoutMilliseconds, actionOnCompletion));
        }

        public void RegisterPendingOperation<T>(WaitHandle waitHandle, Func<T> getResultFunction, int timeoutMilliseconds = 0, Action<bool> actionOnCompletion = null)
        {
            RegisterPendingOperation(new WaitHandlePendingOperation<T>(waitHandle, getResultFunction, timeoutMilliseconds, actionOnCompletion));
        }
        #endregion
    }
}