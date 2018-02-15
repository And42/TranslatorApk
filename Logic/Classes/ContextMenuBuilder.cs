using System;
using System.Windows.Controls;

namespace TranslatorApk.Logic.Classes
{
    internal class ContextMenuBuilder
    {
        internal interface IItemsBuilder
        {
            void Add(object item);
        }

        private class ItemsBuilder : IItemsBuilder
        {
            private readonly ContextMenu _contextMenu;

            public ItemsBuilder(ContextMenu contextMenu)
            {
                _contextMenu = contextMenu ?? throw new ArgumentNullException(nameof(contextMenu));
            }

            public void Add(object item)
            {
                _contextMenu.Items.Add(item);
            }
        }

        private readonly ContextMenu _contextMenu;
        private readonly ItemsBuilder _itemsBuilder;

        public ContextMenuBuilder()
        {
            _contextMenu = new ContextMenu();
            _itemsBuilder = new ItemsBuilder(_contextMenu);
        }

        public IItemsBuilder GetItemsBuilder() => _itemsBuilder;

        public ContextMenu Build() => _contextMenu;
    }
}
