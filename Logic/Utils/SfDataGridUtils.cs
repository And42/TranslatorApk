using System;
using System.Reflection;
using AndroidTranslator.Interfaces.Files;
using AndroidTranslator.Interfaces.Strings;
using Syncfusion.Data;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.UI.Xaml.ScrollAxis;
using UsefulFunctionsLib;

namespace TranslatorApk.Logic.Utils
{
    public static class SfDataGridUtils
    {
        /// <summary>
        /// Возвращает <see cref="DetailsViewManager"/> таблицы
        /// </summary>
        /// <param name="grid">Таблица, объект которой нужно получить</param>
        public static DetailsViewManager GetViewManager(this SfDataGrid grid)
        {
            FieldInfo propertyInfo = grid.GetType().GetField("DetailsViewManager", BindingFlags.Instance | BindingFlags.NonPublic);
            return propertyInfo?.GetValue(grid) as DetailsViewManager;
        }

        /// <summary>
        /// Возвращает таблицу вложенных элементов для таблицы, при этом раскрывая её и проматывая до её начала
        /// </summary>
        /// <param name="grid">Таблица, таблицу вложенных элементов которой нужно получить</param>
        /// <param name="relationalColumn">Название столбца с подробностями</param>
        /// <param name="recordIndex">Позиция записи в таблице</param>
        /// <param name="detailsColumn">Номер столбца в таблице вложенных элементов</param>
        public static SfDataGrid GetDetailsViewGridWUpd(this SfDataGrid grid, string relationalColumn, int recordIndex, int detailsColumn = 1)
        {
            int rowIndex = grid.ResolveToRowIndex(recordIndex);
            RecordEntry record = grid.View.Records[recordIndex];

            if (!record.IsExpanded)
                grid.ExpandDetailsViewAt(recordIndex);

            int vIndex = grid.DetailsViewDefinition.FindIndex(it => it.RelationalColumn == relationalColumn) + rowIndex + 1;

            grid.ScrollInView(new RowColumnIndex(vIndex, detailsColumn));

            SfDataGrid view = grid.GetDetailsViewGrid(recordIndex, relationalColumn);

            if (view != null)
                return view;

            grid.GetViewManager().BringIntoView(vIndex);

            grid.ScrollInView(new RowColumnIndex(rowIndex, detailsColumn));

            return grid.GetDetailsViewGrid(recordIndex, relationalColumn);
        }

        /// <summary>
        /// Перемещает позицию в таблице на позицию необходимой строки и выделяет найденную строку
        /// </summary>
        /// <param name="grid">Таблица для обработки</param>
        /// <param name="filePredicate">Проверка на корректность файла</param>
        /// <param name="stringPredicate">Проверка на корректность строки</param>
        public static void ScrollToFileAndSelectString(this SfDataGrid grid, Predicate<IEditableFile> filePredicate,
            Predicate<IOneString> stringPredicate)
        {
            int parentIndex = grid.View.Records.FindIndex(it => filePredicate(it.Data.As<IEditableFile>()));

            if (parentIndex == -1)
                return;

            SfDataGrid detailsGrid = grid.GetDetailsViewGridWUpd("Details", parentIndex);

            VisualContainer container = grid.GetVisualContainer();

            container.ScrollRows.ScrollInView(grid.ResolveToRowIndex(parentIndex));

            int childIndex = detailsGrid.View.Records.FindIndex(it => stringPredicate(it.Data as IOneString));

            if (childIndex == -1)
                return;

            detailsGrid.SelectedIndex = childIndex;

            for (int i = 0; i < childIndex; i++)
                container.ScrollRows.ScrollToNextLine();
        }

        /// <summary>
        /// Перемещает позицию в таблице на позицию необходимого файла и выделяет найденный файл
        /// </summary>
        /// <param name="grid">Таблица для обработки</param>
        /// <param name="filePredicate">Проверка на корректность файла</param>
        /// <param name="expandRecord">Нужно ли разворачивать таблицу вложенных элементов у файла</param>
        public static void ScrollToFileAndSelect(this SfDataGrid grid, Predicate<IEditableFile> filePredicate, bool expandRecord = true)
        {
            int parentIndex = grid.View.Records.FindIndex(it => filePredicate(it.Data.As<IEditableFile>()));

            if (parentIndex == -1)
                return;

            if (expandRecord)
                grid.ExpandDetailsViewAt(parentIndex);

            grid.SelectedIndex = parentIndex;

            VisualContainer container = grid.GetVisualContainer();

            container.ScrollRows.ScrollInView(grid.ResolveToRowIndex(parentIndex));
        }
    }
}
