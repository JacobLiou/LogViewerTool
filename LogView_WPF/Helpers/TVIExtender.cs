﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace LogViewer.Helpers
{
    public class TVIExtender
    {
        private TreeViewItem item;

        public static DependencyProperty UseExtenderProperty =
            DependencyProperty.RegisterAttached("UseExtender", typeof(bool), typeof(TVIExtender),
                new PropertyMetadata(false, OnChangedUseExtender));

        public static bool GetUseExtender(DependencyObject sender)
        {
            return (bool)sender.GetValue(UseExtenderProperty);
        }

        public static void SetUseExtender(DependencyObject sender, bool useExtender)
        {
            sender.SetValue(UseExtenderProperty, useExtender);
        }

        private static void OnChangedUseExtender(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;
            if (null != item)
            {
                if ((bool)e.NewValue)
                {
                    if (item.ReadLocalValue(ItemExtenderProperty) == DependencyProperty.UnsetValue)
                    {
                        TVIExtender extender = new TVIExtender(item);
                        item.SetValue(ItemExtenderProperty, extender);
                    }
                }
                else
                {
                    if (item.ReadLocalValue(ItemExtenderProperty) != DependencyProperty.UnsetValue)
                    {
                        TVIExtender extender = (TVIExtender)item.ReadLocalValue(ItemExtenderProperty);
                        extender.Detach();
                        item.SetValue(ItemExtenderProperty, DependencyProperty.UnsetValue);
                    }
                }
            }
        }

        public static DependencyProperty ItemExtenderProperty =
            DependencyProperty.RegisterAttached("ItemExtender", typeof(TVIExtender), typeof(TVIExtender));

        public static DependencyProperty IsLastOneProperty =
            DependencyProperty.RegisterAttached("IsLastOne", typeof(bool), typeof(TVIExtender));

        public static bool GetIsLastOne(DependencyObject sender)
        {
            return (bool)sender.GetValue(IsLastOneProperty);
        }

        public static void SetIsLastOne(DependencyObject sender, bool isLastOne)
        {
            sender.SetValue(IsLastOneProperty, isLastOne);
        }

        public TVIExtender(TreeViewItem item)
        {
            this.item = item;

            ItemsControl ic = ItemsControl.ItemsControlFromItemContainer(this.item);
            ic.ItemContainerGenerator.ItemsChanged += OnItemsChangedItemContainerGenerator;

            this.item.SetValue(IsLastOneProperty,
                ic.ItemContainerGenerator.IndexFromContainer(this.item) == ic.Items.Count - 1);
        }

        private void OnItemsChangedItemContainerGenerator(object sender, ItemsChangedEventArgs e)
        {
            ItemsControl ic = ItemsControl.ItemsControlFromItemContainer(item);

            if (null != ic)
                item.SetValue(IsLastOneProperty,
                    ic.ItemContainerGenerator.IndexFromContainer(item) == ic.Items.Count - 1);
        }

        private void Detach()
        {
            if (item != null)
            {
                ItemsControl ic = ItemsControl.ItemsControlFromItemContainer(item);
                if (ic == null) return;
                ic.ItemContainerGenerator.ItemsChanged -= OnItemsChangedItemContainerGenerator;

                item = null;
            }
        }
    }
}