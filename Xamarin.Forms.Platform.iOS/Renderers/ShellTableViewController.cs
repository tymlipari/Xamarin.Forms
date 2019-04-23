using CoreGraphics;
using System;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	public class ShellTableViewController : UITableViewController
	{
		readonly IShellContext _context;
		readonly UIView _headerView;
		readonly ShellTableViewSource _source;
		double _headerMax = 200;
		double _headerMin = 44;
		double _headerOffset = 0;
		double _headerSize;

		public ShellTableViewController(IShellContext context, UIView headerView, Action<Element> onElementSelected)
		{
			if (headerView == null)
			{
				_headerMax = 20;
				_headerMin = 0;
			}

			_headerSize = _headerMax;
			_context = context;
			_headerView = headerView;
			_source = new ShellTableViewSource(context, onElementSelected);
			_source.ScrolledEvent += OnScrolled;

			((IShellController)_context.Shell).StructureChanged += OnStructureChanged;
		}

		void OnStructureChanged(object sender, EventArgs e)
		{
			_source.ClearCache();
			TableView.ReloadData();
		}

		public void LayoutParallax()
		{
			var parent = TableView.Superview;

			TableView.Frame = parent.Bounds;
			if (_headerView != null)
				_headerView.Frame = new CGRect(0, _headerOffset + SafeAreaOffset, parent.Frame.Width, _headerSize);
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
			TableView.Source = _source;
		}

		public override void ViewDidAppear(bool animated)
		{
			base.ViewDidAppear(animated);
			if (Forms.IsiOS11OrNewer)
				TableView.ContentInsetAdjustmentBehavior = UIScrollViewContentInsetAdjustmentBehavior.Never;
			TableView.ContentInset = new UIEdgeInsets((nfloat)_headerMax + SafeAreaOffset, 0, 0, 0);
		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
		}

		protected override void Dispose(bool disposing)
		{
			if(disposing)
			{
				if ((_context?.Shell as IShellController) != null)
					((IShellController)_context.Shell).StructureChanged -= OnStructureChanged;
			}
			
			base.Dispose(disposing);
		}

		void OnScrolled(object sender, UIScrollView e)
		{
			var headerBehavior = _context.Shell.FlyoutHeaderBehavior;

			switch (headerBehavior)
			{
				case FlyoutHeaderBehavior.Default:
				case FlyoutHeaderBehavior.Fixed:
					_headerSize = _headerMax;
					break;

				case FlyoutHeaderBehavior.Scroll:
					_headerSize = _headerMax;
					_headerOffset = Math.Min (0, -(_headerMax + e.ContentOffset.Y));
					break;

				case FlyoutHeaderBehavior.CollapseOnScroll:
					_headerSize = Math.Max(_headerMin, Math.Min(_headerMax, _headerMax - e.ContentOffset.Y - _headerMax));
					break;
			}

			LayoutParallax();
		}

		float SafeAreaOffset
		{
			get
			{
				if (!Forms.IsiOS11OrNewer)
					return 0f;

				return (float)(UIApplication.SharedApplication.KeyWindow?.SafeAreaInsets.Top ?? 0);
			}
		}
	}
}