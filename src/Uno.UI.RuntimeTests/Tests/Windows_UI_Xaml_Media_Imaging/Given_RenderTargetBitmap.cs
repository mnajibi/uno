﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Private.Infrastructure;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Graphics.Display;
using Windows.UI;
using Windows.UI.Xaml.Media;
using FluentAssertions;
using Uno.UI.RuntimeTests.Helpers;

namespace Uno.UI.RuntimeTests.Tests.Windows_UI_Xaml_Media_Imaging
{
	[TestClass]
	[RunsOnUIThread]
	public class Given_RenderTargetBitmap
	{
		private static readonly Windows.UI.Xaml.Media.SolidColorBrush Background = new(Windows.UI.Color.FromArgb(255, 0, 0, 255));
		private static readonly Windows.UI.Xaml.Media.SolidColorBrush BorderBrush = new(Windows.UI.Color.FromArgb(125, 125, 0, 0));
		private static readonly System.Numerics.Vector2 BorderSize = new(10, 10);

		[TestMethod]
#if __WASM__
		[Ignore("Not implemented yet.")]
#endif
#if __MACOS__
		[Ignore("Currently fails on macOS, part of #9282 epic")]
#endif
#if __IOS__
		[Ignore("Currently fails on iOS: https://github.com/unoplatform/uno/issues/9080")]
#endif
		public async Task When_Render_Border_GetPixelsAsync()
		{
			if (DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel is not 1.0)
			{
				Assert.Inconclusive("This test is not compatible with non-default display scaling.");
				return;
			}

			var border = new Border()
			{
				Name = "TestBorder",
				Width = 10,
				Height = 10,
				BorderThickness = new Thickness(1),
				Background = Background,
				BorderBrush = BorderBrush,
			};

			var resourceName = GetType()
				.Assembly
				.GetManifestResourceNames()
				.FirstOrDefault(name => name.EndsWith("Border_Snapshot.bgra8"));

			Assert.IsNotNull(resourceName, "Do not find resource named Border_Snapshot.bgra8");

			var rawBorderSnapshot = GetType()
				.Assembly
				.GetManifestResourceStream(resourceName)
				.ReadAllBytes();

			await UITestHelper.Load(border);

			Assert.AreEqual(BorderSize, border.ActualSize, "Invalid Layouted.");

			var renderer = new RenderTargetBitmap();
			await renderer.RenderAsync(border);

			var pixels = (await renderer.GetPixelsAsync()).ToArray();

			//Using of the loop instead of IsSameData method to get more information in case of failure.
			Assert.AreEqual(rawBorderSnapshot.Length, pixels.Length, $"Invalid length. Expected {rawBorderSnapshot.Length} found {pixels.Length}.");

			for (var i = 0; i < pixels.Length; i += 4)
			{
				Assert.AreEqual(rawBorderSnapshot[i + 3], pixels[i + 3], $"The A channel of pixel {i / 4} is not same. Expected {rawBorderSnapshot[i + 3]:x2} found {pixels[i + 3]:x2}.");
				Assert.AreEqual(rawBorderSnapshot[i + 2], pixels[i + 2], $"The R channel of pixel {i / 4} is not same. Expected {rawBorderSnapshot[i + 2]:x2} found {pixels[i + 2]:x2}.");
				Assert.AreEqual(rawBorderSnapshot[i + 1], pixels[i + 1], $"The G channel of pixel {i / 4} is not same. Expected {rawBorderSnapshot[i + 1]:x2} found {pixels[i + 1]:x2}.");
				Assert.AreEqual(rawBorderSnapshot[i], pixels[i], $"The B channel of pixel {i / 4} is not same. Expected {rawBorderSnapshot[i]:x2} found {pixels[i]:x2}.");
			}
		}

		[TestMethod]
#if __WASM__
		[Ignore("Not implemented yet.")]
#elif __MACOS__
		[Ignore("Currently fails on macOS, part of #9282 epic")]
#endif
		public async Task When_Render_Then_CanRenderOnCanvas()
		{
			var border = new Border()
			{
				Name = "TestBorder",
				Width = 10,
				Height = 10,
				BorderThickness = new Thickness(1),
				Background = Background,
				BorderBrush = BorderBrush
			};

			await UITestHelper.Load(border);

			var sut = new RenderTargetBitmap();
			await sut.RenderAsync(border);

			var onCanvasReady = new TaskCompletionSource<object>();
			var onCanvasTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(Debugger.IsAttached ? 300 : 1));
			var onCanvas = new Image
			{
				Width = 10,
				Height = 10
			};
			onCanvasTimeout.Token.Register(() => onCanvasReady.TrySetException(new TimeoutException("Image didn't render")));
			onCanvas.ImageOpened += (snd, e) => onCanvasReady.TrySetResult(default);
			onCanvas.ImageFailed += (snd, e) => onCanvasReady.TrySetException(new InvalidOperationException(e.ErrorMessage));
			onCanvas.Source = sut;

			TestServices.WindowHelper.WindowContent = onCanvas;

			await onCanvasReady.Task;
			await TestServices.WindowHelper.WaitForLoaded(onCanvas);
			await TestServices.WindowHelper.WaitForIdle();

			// We are also using RenderTargetBitmap to validate the result ... weird but it works :)
			var result = await UITestHelper.ScreenShot(onCanvas);

			ImageAssert.HasColorAt(result, 5, 5, Background.Color);
		}

		[TestMethod]
#if __WASM__
		[Ignore("Not implemented yet.")]
#elif __MACOS__
		[Ignore("Currently fails on macOS, part of #9282 epic")]
#endif
		public async Task When_RenderNotOpaqueContent_Then_ImageNotOpaque()
		{
			var nonOpaqueColor = Color.FromArgb(0x80, 0xCC, 0x00, 0x00);
			var border = new Border()
			{
				Name = "TestBorder",
				Width = 10,
				Height = 10,
				Background = new SolidColorBrush(nonOpaqueColor),
			};

			await UITestHelper.Load(border);

			// Note: We do not use the UITestHelper.ScreenShot to ensure usage of the RenderTargetBitmap!
			var sut = new RenderTargetBitmap();
			await sut.RenderAsync(border);
			var result = await RawBitmap.From(sut, border);

			ImageAssert.HasColorAt(result, 5, 5, nonOpaqueColor, tolerance: 1);
		}
	}
}
