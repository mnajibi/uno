﻿using Uno.Extensions;
using Uno.Foundation.Logging;
using Uno.UI.Controls;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Uno.UI.DataBinding;
using Uno.Disposables;
using Windows.UI.Xaml.Data;
using System.Runtime.CompilerServices;
using System.Drawing;
using Uno.UI;
using Windows.UI.Xaml.Media;

using View = Windows.UI.Xaml.UIElement;
using Uno.UI.Helpers;

namespace Windows.UI.Xaml.Controls
{
	public partial class Panel : IEnumerable
	{
		private Action _borderBrushChanged;

		public Panel()
		{
			Initialize();
			this.SizeChanged += (_, _) => UpdateBorder();
		}

		partial void Initialize();

		partial void UpdateBorder()
		{
			SetBorder(BorderThicknessInternal, BorderBrushInternal, CornerRadiusInternal);
		}

		protected virtual void OnChildrenChanged()
		{
			UpdateBorder();
		}

		partial void OnPaddingChangedPartial(Thickness oldValue, Thickness newValue)
		{
			UpdateBorder();
		}

		partial void OnBorderBrushChangedPartial(Brush oldValue, Brush newValue)
		{
			var newOnInvalidateRender = _borderBrushChanged ?? (() => UpdateBorder());
			Brush.SetupBrushChanged(oldValue, newValue, ref _borderBrushChanged, newOnInvalidateRender);
		}

		partial void OnBorderThicknessChangedPartial(Thickness oldValue, Thickness newValue)
		{
			UpdateBorder();
		}

		partial void OnCornerRadiusChangedPartial(CornerRadius oldValue, CornerRadius newValue)
		{
			UpdateBorder();
		}

		/// <summary>        
		/// Support for the C# collection initializer style.
		/// Allows items to be added like this 
		/// new Panel 
		/// {
		///    new Border()
		/// }
		/// </summary>
		/// <param name="view"></param>
		public void Add(View view)
		{
			Children.Add(view);
		}

		public new IEnumerator GetEnumerator()
		{
			return this.GetChildren().GetEnumerator();
		}

		protected override void OnBackgroundChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnBackgroundChanged(e);
			UpdateHitTest();
		}

		bool ICustomClippingElement.AllowClippingToLayoutSlot => true;
		bool ICustomClippingElement.ForceClippingToLayoutSlot => CornerRadiusInternal != CornerRadius.None;
	}
}
