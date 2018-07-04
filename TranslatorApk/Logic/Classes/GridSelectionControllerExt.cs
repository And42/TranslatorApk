using System;
using System.Windows.Input;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.ScrollAxis;

namespace TranslatorApk.Logic.Classes
{
    public class GridSelectionControllerExt : GridCellSelectionController
    {
        private readonly Func<KeyEventArgs, bool> _keyDown;
        private readonly Func<GridPointerEventArgs, RowColumnIndex, bool> _pointerOperation;

        public GridSelectionControllerExt(SfDataGrid datagrid, Func<KeyEventArgs, bool> keyDown, Func<GridPointerEventArgs, RowColumnIndex, bool> pointerOperation = null) : base(datagrid)
        {
            _keyDown = keyDown;
            _pointerOperation = pointerOperation;
        }

        protected override void ProcessKeyDown(KeyEventArgs args)
        {
            if (!_keyDown(args))
                base.ProcessKeyDown(args);
        }

        public override void HandlePointerOperations(GridPointerEventArgs args, RowColumnIndex rowColumnIndex)
        {
            if (_pointerOperation != null && _pointerOperation(args, rowColumnIndex))
            {
                return;
            }

            base.HandlePointerOperations(args, rowColumnIndex);
        }
    }
}
