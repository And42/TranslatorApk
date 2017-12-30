﻿using System;
using System.Threading.Tasks;
using TranslatorApk.Logic.Interfaces;

namespace TranslatorApk.Logic.Classes
{
    public abstract class ViewModelBase : BindableBase, IViewModelBase
    {
        protected class LoadingDisposable : IDisposable
        {
            private IViewModelBase _model;

            public LoadingDisposable(IViewModelBase model)
            {
                if (model == null)
                    throw new ArgumentNullException(nameof(model));
                if (model.IsLoading)
                    throw new InvalidOperationException(nameof(model) + " is loading at the moment");

                _model = model;
                _model.IsLoading = true;
            }

            public void Dispose()
            {
                if (_model == null)
                    throw new ObjectDisposedException(nameof(LoadingDisposable));

                _model.IsLoading = false;
                _model = null;
            }
        }

        protected static readonly Task EmptyTask = new Task(() => {});

        private bool _isLoading;

        public virtual bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public virtual Task LoadItems() => EmptyTask;

        public abstract void UnsubscribeFromEvents();
    }
}