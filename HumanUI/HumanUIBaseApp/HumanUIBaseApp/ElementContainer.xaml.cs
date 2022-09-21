using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HumanUIBaseApp
{
    /// <summary>
    /// ElementContainer.xaml 的交互逻辑
    /// </summary>
    public partial class ElementContainer : UserControl
    {
        public ElementContainer()
        {
            this.InheritanceBehavior = InheritanceBehavior.SkipToThemeNext;
            InitializeComponent();
        }


        /// <summary>
        /// Sets the data context.
        /// </summary>
        /// <param name="o">The object.</param>
        public void setDataContext(object o)
        {
            this.DataContext = o;
        }

        public bool HorizontalScrollingEnabled
        {
            get => MasterScrollViewer.HorizontalScrollBarVisibility == ScrollBarVisibility.Auto;
            set => MasterScrollViewer.HorizontalScrollBarVisibility = value ? ScrollBarVisibility.Auto : ScrollBarVisibility.Disabled;
        }


        /// <summary>
        /// Sets the font of most text elements in the window.
        /// </summary>
        /// <param name="fontName">Name of the font.</param>
        public void setFont(string fontName)
        {
            FontFamily ff = new System.Windows.Media.FontFamily(fontName);
            TextElement.SetFontFamily(MasterStackPanel, ff);
        }


        /// <summary>
        /// Adds the element to the master stack panel.
        /// </summary>
        /// <param name="elem">The element to add.</param>
        /// <returns>The index of the added element</returns>
        public int AddElement(UIElement elem)
        {
            return MasterStackPanel.Children.Add(elem);
        }

        /// <summary>
        /// Adds the element to the master stack panel at the specified index.
        /// </summary>
        /// <param name="elem">The element.</param>
        /// <param name="index">The index.</param>
        public void AddElement(UIElement elem, int index)
        {
            MasterStackPanel.Children.Insert(index, elem);
        }

        /// <summary>
        /// Removes the element from the master stack panel.
        /// </summary>
        /// <param name="elem">The element.</param>
        /// <returns>the index of the removed element.</returns>
        public int RemoveFromStack(UIElement elem)
        {
            int i = MasterStackPanel.Children.IndexOf(elem);
            MasterStackPanel.Children.Remove(elem);
            return i;
        }

        /// <summary>
        /// Adds the element to the main grid.
        /// </summary>
        /// <param name="elem">The element.</param>
        /// <param name="zIndex">Z index of the element</param>
        public void AddToGrid(UIElement elem, int zIndex)
        {
            AbsPosGrid.Children.Add(elem);
            Grid.SetZIndex(elem, zIndex);
        }

        /// <summary>
        /// Removes the element from the grid.
        /// </summary>
        /// <param name="elem">The element.</param>
        /// <returns>the z index of the removed element</returns>
        public int RemoveFromGrid(UIElement elem)
        {
            int i = Grid.GetZIndex(elem);
            AbsPosGrid.Children.Remove(elem);
            return i;
        }

        /// <summary>
        /// Moves the element from stack to grid.
        /// </summary>
        /// <param name="elem">The element.</param>
        public void MoveFromStackToGrid(UIElement elem)
        {
            int index = RemoveFromStack(elem);
            AddToGrid(elem, index);
        }

        /// <summary>
        /// Moves the element from grid to stack.
        /// </summary>
        /// <param name="elem">The element.</param>
        public void MoveFromGridToStack(UIElement elem)
        {
            int index = RemoveFromGrid(elem);
            AddElement(elem, index);
        }


        /// <summary>
        /// Clears the elements from the grid and the master stack panel, then replaces the master stack panel in the grid.
        /// </summary>
        public void clearElements()
        {
            MasterStackPanel.Children.Clear();
            AbsPosGrid.Children.Clear();
            AbsPosGrid.Children.Add(MasterStackPanel);
        }
    }
}
