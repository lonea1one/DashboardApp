﻿using Microsoft.Xaml.Behaviors;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;

namespace DashboardProjects.Behaviors;

public class ListBoxSelectionBehavior : Behavior<ListBox>
{
    public static readonly DependencyProperty SelectedItemsProperty =
        DependencyProperty.Register(
            nameof(SelectedItems),
            typeof(ObservableCollection<int>),
            typeof(ListBoxSelectionBehavior),
            new PropertyMetadata(null, OnSelectedItemsChanged));

    public ObservableCollection<int> SelectedItems
    {
        get => (ObservableCollection<int>)GetValue(SelectedItemsProperty);
        set => SetValue(SelectedItemsProperty, value);
    }

	protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.MouseEnter += OnMouseEnter;
        AssociatedObject.MouseLeave += OnMouseLeave;
        SelectedItems.CollectionChanged += OnSelectedItemsCollectionChanged;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.MouseEnter -= OnMouseEnter;
        AssociatedObject.MouseLeave -= OnMouseLeave;
        SelectedItems.CollectionChanged -= OnSelectedItemsCollectionChanged;
    }

    private void OnMouseEnter(object sender, MouseEventArgs e)
    {
        Mouse.OverrideCursor = Cursors.Hand;
    }

    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
        Mouse.OverrideCursor = Cursors.Arrow;
    }

    private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var behavior = (ListBoxSelectionBehavior)d;
        if (e.OldValue is ObservableCollection<int> oldCollection)
        {
            oldCollection.CollectionChanged -= behavior.OnSelectedItemsCollectionChanged;
        }
        if (e.NewValue is ObservableCollection<int> newCollection)
        {
            newCollection.CollectionChanged += behavior.OnSelectedItemsCollectionChanged;
        }
    }

    private void OnSelectedItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
		if (AssociatedObject == null) return;

		AssociatedObject.SelectedItems.Clear();
        foreach (var index in SelectedItems)
        {
            AssociatedObject.SelectedItems.Add(AssociatedObject.Items[index]);
        }
	}

}
