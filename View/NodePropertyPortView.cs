using NodeGraph.Controls;
using NodeGraph.Converters;
using NodeGraph.Model;
using PropertyTools.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace NodeGraph.View
{
	[TemplatePart( Name = "PART_Header", Type = typeof( EditableTextBlock ) )]
	public class NodePropertyPortView : NodePortView
	{
		#region Fields

		private EditableTextBlock _Part_Header;
		private DispatcherTimer _ClickTimer = new DispatcherTimer();
		private int _ClickCount = 0;

		#endregion // Fields

		#region Properites

		public Visibility PropertyEditorVisibility
		{
			get { return ( Visibility )GetValue( PropertyEditorVisibilityProperty ); }
			set { SetValue( PropertyEditorVisibilityProperty, value ); }
		}
		public static readonly DependencyProperty PropertyEditorVisibilityProperty =
			DependencyProperty.Register( "PropertyEditorVisibility", typeof( Visibility ), typeof( NodePropertyPortView ), new PropertyMetadata( Visibility.Hidden ) );

		public FrameworkElement PropertyEditor
		{
			get { return ( FrameworkElement )GetValue( PropertyEditorProperty ); }
			set { SetValue( PropertyEditorProperty, value ); }
		}
		public static readonly DependencyProperty PropertyEditorProperty =
			DependencyProperty.Register( "PropertyEditor", typeof( FrameworkElement ), typeof( NodePropertyPortView ), new PropertyMetadata( null ) );

		#endregion // Properties

		#region Constructor

		public NodePropertyPortView( bool isInput ) : base( isInput )
		{
			Loaded += NodePropertyPortView_Loaded;
		}

		#endregion // Constructor

		#region Template Events

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_Part_Header = Template.FindName( "PART_Header", this ) as EditableTextBlock;
			if ( null != _Part_Header )
			{
				_Part_Header.MouseDown += _Part_Header_MouseDown; ;
			}
		}

		#endregion // Template Events

		#region Header Events

		private void _Part_Header_MouseDown( object sender, MouseButtonEventArgs e )
		{
			Keyboard.Focus( _Part_Header );

			if ( 0 == _ClickCount )
			{
				_ClickTimer.Start();
				_ClickCount++;
			}
			else if ( 1 == _ClickCount )
			{
				_Part_Header.IsEditing = true;
				Keyboard.Focus( _Part_Header );
				_ClickCount = 0;
				_ClickTimer.Stop();

				e.Handled = true;
			}
		}

		#endregion // Header Events

		#region Events

		private void NodePropertyPortView_Loaded( object sender, RoutedEventArgs e )
		{
			CreatePropertyEditor();
			SynchronizeProperties();

			_ClickTimer.Interval = TimeSpan.FromMilliseconds( 300 );
			_ClickTimer.Tick += _ClickTimer_Tick; ;
		}

		private void _ClickTimer_Tick( object sender, EventArgs e )
		{
			_ClickCount = 0;
			_ClickTimer.Stop();
		}

		protected override void SynchronizeProperties()
		{
			base.SynchronizeProperties();

			if( IsInput )
			{
				PropertyEditorVisibility = IsFilledPort ? Visibility.Collapsed : Visibility.Visible;
			}
		}

		#endregion // Events

		#region Property Editors

		private void CreatePropertyEditor()
		{
			NodePropertyPort port = ViewModel.Model as NodePropertyPort;
			if( port.HasEditor )
			{
				Type type = port.ValueType;

				if( typeof( bool ) == type )
				{
					PropertyEditor = CreateBoolEditor();
				}
				else if( typeof( string ) == type )
				{
					PropertyEditor = CreateStringEditor();
				}
				else if( typeof( byte ) == type )
				{
					PropertyEditor = CreateByteEditor();
				}
				else if( typeof( short ) == type )
				{
					PropertyEditor = CreateShortEditor();
				}
				else if( typeof( int ) == type )
				{
					PropertyEditor = CreateIntEditor();
				}
				else if( typeof( long ) == type )
				{
					PropertyEditor = CreateLongEditor();
				}
				else if( typeof( float ) == type )
				{
					PropertyEditor = CreateFloatEditor();
				}
				else if( typeof( double ) == type )
				{
					PropertyEditor = CreateDoubleEditor();
				}
				else if( type.IsEnum )
				{
					PropertyEditor = CreateEnumEditor();
				}
				else if( typeof( Color ) == type )
				{
					PropertyEditor = CreateColorEditor();
				}

				if( null != PropertyEditor )
				{
					PropertyEditorVisibility = Visibility.Visible;
				}
			}
		}

		public FrameworkElement CreateEnumEditor()
		{
			NodePropertyPort port = ViewModel.Model as NodePropertyPort;

			Array array = Enum.GetValues( port.ValueType );
			int selectedIndex = -1;
			for( int i = 0; i < array.Length; ++i )
			{
				if( port.Value.ToString() == array.GetValue( i ).ToString() )
				{
					selectedIndex = i;
					break;
				}
			}

			ComboBox comboBox = new ComboBox();
			comboBox.SelectionChanged += EnumComboBox_SelectionChanged;
			comboBox.ItemsSource = array;
			comboBox.SelectedIndex = selectedIndex;
			return comboBox;
		}

		private void EnumComboBox_SelectionChanged( object sender, SelectionChangedEventArgs e )
		{
			NodePropertyPort port = ViewModel.Model as NodePropertyPort;

			ComboBox comboBox = PropertyEditor as ComboBox;
			if( null != comboBox )
			{
				port.Value = comboBox.SelectedItem;
			}
		}

		public FrameworkElement CreateBoolEditor()
		{
			NodePropertyPort port = ViewModel.Model as NodePropertyPort;

			CheckBox textBox = new CheckBox();
			textBox.IsChecked = ( bool )port.Value;
			textBox.SetBinding( CheckBox.IsCheckedProperty, CreateBinding( port, "Value", null ) );
			return textBox;
		}

		public FrameworkElement CreateStringEditor()
		{
			NodePropertyPort port = ViewModel.Model as NodePropertyPort;

			TextBoxEx textBox = new TextBoxEx();
			textBox.Text = port.Value.ToString();
			textBox.SetBinding( TextBox.TextProperty, CreateBinding( port, "Value", null ) );
			return textBox;
		}

		public FrameworkElement CreateByteEditor()
		{
			NodePropertyPort port = ViewModel.Model as NodePropertyPort;

			NumericTextBox textBox = new NumericTextBox();
			textBox.IsInteger = true;
			textBox.Text = port.Value.ToString();
			textBox.SetBinding( TextBox.TextProperty, CreateBinding( port, "Value", new ByteToStringConverter() ) );
			return textBox;
		}

		public FrameworkElement CreateShortEditor()
		{
			NodePropertyPort port = ViewModel.Model as NodePropertyPort;

			NumericTextBox textBox = new NumericTextBox();
			textBox.IsInteger = true;
			textBox.Text = port.Value.ToString();
			textBox.SetBinding( TextBox.TextProperty, CreateBinding( port, "Value", new ShortToStringConverter() ) );
			return textBox;
		}

		public FrameworkElement CreateIntEditor()
		{
			NodePropertyPort port = ViewModel.Model as NodePropertyPort;

			NumericTextBox textBox = new NumericTextBox();
			textBox.IsInteger = true;
			textBox.Text = port.Value.ToString();
			textBox.SetBinding( TextBox.TextProperty, CreateBinding( port, "Value", new IntToStringConverter() ) );
			return textBox;
		}

		public FrameworkElement CreateLongEditor()
		{
			NodePropertyPort port = ViewModel.Model as NodePropertyPort;

			NumericTextBox textBox = new NumericTextBox();
			textBox.IsInteger = true;
			textBox.Text = port.Value.ToString();
			textBox.SetBinding( TextBox.TextProperty, CreateBinding( port, "Value", new LongToStringConverter() ) );
			return textBox;
		}

		public FrameworkElement CreateFloatEditor()
		{
			NodePropertyPort port = ViewModel.Model as NodePropertyPort;

			NumericTextBox textBox = new NumericTextBox();
			textBox.IsInteger = false;
			textBox.Text = port.Value.ToString();
			textBox.SetBinding( TextBox.TextProperty, CreateBinding( port, "Value", new FloatToStringConverter() ) );
			return textBox;
		}

		public FrameworkElement CreateDoubleEditor()
		{
			NodePropertyPort port = ViewModel.Model as NodePropertyPort;

			NumericTextBox textBox = new NumericTextBox();
			textBox.IsInteger = false;
			textBox.Text = port.Value.ToString();
			textBox.SetBinding( TextBox.TextProperty, CreateBinding( port, "Value", new DoubleToStringConverter() ) );
			return textBox;
		}

		public FrameworkElement CreateColorEditor()
		{
			NodePropertyPort port = ViewModel.Model as NodePropertyPort;

			ColorPicker picker = new ColorPicker();
			picker.SelectedColor = ( Color )port.Value;
			picker.SetBinding( ColorPicker.SelectedColorProperty, CreateBinding( port, "Value", null ) );
			return picker;
		}

		public Binding CreateBinding( NodePropertyPort port, string propertyName, IValueConverter converter )
		{
			var binding = new Binding( propertyName )
			{
				Source = port,
				Mode = BindingMode.TwoWay,
				Converter = converter,
				UpdateSourceTrigger = UpdateSourceTrigger.Default,
				ValidatesOnDataErrors = true,
				ValidatesOnExceptions = true
			};

			return binding;
		}

		#endregion // Property Editors.
	}
}
