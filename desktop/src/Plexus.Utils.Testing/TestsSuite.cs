/**
 * Copyright 2017 Plexus Interop Deutsche Bank AG
 * SPDX-License-Identifier: Apache-2.0
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
﻿namespace Plexus
{
    using Shouldly;
    using System;
    using System.Collections.Concurrent;
    using System.Security.Cryptography;
    using System.Threading.Tasks;
    using Xunit;
    using Xunit.Abstractions;

    [Collection("Default")]
    [DisplayTestMethodName]
    public abstract class TestsSuite : IDisposable
    {
        private readonly ConcurrentStack<IDisposable> _disposables = new ConcurrentStack<IDisposable>();

        protected static readonly Random Random = new Random(42);

        protected static readonly MD5 Md5 = MD5.Create();

        protected static readonly TimeSpan Timeout1Sec = TimeSpan.FromSeconds(1);
        protected static readonly TimeSpan Timeout5Sec = TimeSpan.FromSeconds(5);
        protected static readonly TimeSpan Timeout10Sec = TimeSpan.FromSeconds(10);

        protected ITestOutputHelper Console { get; }

        protected TestsSuite() : this(null)
        {
        }

        protected TestsSuite(ITestOutputHelper output)
        {
            Console = output;
            Log = LogManager.GetLogger(GetType());
        }

        protected ILogger Log { get; }

        protected T RegisterDisposable<T>(T disposable) where T : IDisposable
        {
            _disposables.Push(disposable);
            return disposable;
        }

        protected void RunWith10SecTimeout(Func<Task> func) => RunWithTimeout(Timeout10Sec, func);
        protected void RunWith10SecTimeout(Action action) => RunWithTimeout(Timeout10Sec, action);
        protected void RunWith5SecTimeout(Func<Task> func) => RunWithTimeout(Timeout5Sec, func);
        protected void RunWith1SecTimeout(Func<Task> func) => RunWithTimeout(Timeout1Sec, func);

        protected void RunWithTimeout(TimeSpan timeout, Func<Task> func) =>
            Should.CompleteIn(() => TaskRunner.RunInBackground(func), timeout);

        protected void RunWithTimeout(TimeSpan timeout, Action action) =>
            Should.CompleteIn(action, timeout);

        public virtual void Dispose()
        {
            RunWith10SecTimeout(
                () =>
                {
                    Log.Info("Disposing test resources");
                    while (_disposables.TryPop(out var disposable))
                    {
                        disposable.Dispose();
                    }
                });
        }
    }
}