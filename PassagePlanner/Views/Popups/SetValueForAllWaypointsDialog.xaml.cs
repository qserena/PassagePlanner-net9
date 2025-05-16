using MahApps.Metro.Controls;
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
using System.Windows.Shapes;

using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Globalization;

namespace PassagePlanner
{

    public enum ValidationRuleType
    {
        NoValidation,

        SpeedRule,

        WaterDepthRule
    }

    public enum InputModeType
    {
        Text,

        Numeric
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class SetValueForAllWaypointsDialog : MetroWindow
    {
        ValidationRuleType _validationType = ValidationRuleType.NoValidation;

        public SetValueForAllWaypointsDialog(string columnHeaderText, ValidationRuleType validationRule, bool onlySelectedWaypoints)
        {
            InitializeComponent();

            string valueOrTextString = "value";
            string allOrSelectedString = "all";

            _validationType = validationRule;

            textBlockColumnHeaderText.Text = columnHeaderText + ":";

            if (onlySelectedWaypoints)
            {
                allOrSelectedString = "selected";
            }
            else
            {
                allOrSelectedString = "all";
            }

            switch (_validationType)
            {
                case ValidationRuleType.NoValidation:
                    valueOrTextString = "text";
                    textBoxWaterDepthRule.Visibility = System.Windows.Visibility.Collapsed;
                    textBoxSpeedRule.Visibility = System.Windows.Visibility.Collapsed;
                    textBoxNoValidationRule.Focus();
                    break;

                case ValidationRuleType.SpeedRule:
                    valueOrTextString = "value";
                    textBoxWaterDepthRule.Visibility = System.Windows.Visibility.Collapsed;
                    textBoxNoValidationRule.Visibility = System.Windows.Visibility.Collapsed;
                    textBoxSpeedRule.Focus();
                    break;

                case ValidationRuleType.WaterDepthRule:
                    valueOrTextString = "value";
                    textBoxSpeedRule.Visibility = System.Windows.Visibility.Collapsed;
                    textBoxNoValidationRule.Visibility = System.Windows.Visibility.Collapsed;
                    textBoxWaterDepthRule.Focus();
                    break;

                default:
                    break;
            }

            Title = "Set " + valueOrTextString + " for " + allOrSelectedString + " waypoints";

            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void buttonOkYes_Click(object sender, RoutedEventArgs e)
        {
            ValidationResult validationResult = null;

            switch (_validationType)
            {
                case ValidationRuleType.NoValidation:
                    validationResult = ValidationResult.ValidResult;
                    this.Tag = textBoxNoValidationRule.Text;
                    break;

                case ValidationRuleType.SpeedRule:
                    // Force validation
                    textBoxSpeedRule.GetBindingExpression(TextBox.TextProperty).UpdateSource();
                    SpeedRule speedRule = new SpeedRule();
                    validationResult = speedRule.Validate(textBoxSpeedRule.Text, CultureInfo.CurrentCulture);
                    this.Tag = textBoxSpeedRule.Text;
                    break;

                case ValidationRuleType.WaterDepthRule:
                    // Force validation
                    textBoxWaterDepthRule.GetBindingExpression(TextBox.TextProperty).UpdateSource();
                    WaterDepthRule waterDepthRule = new WaterDepthRule();
                    validationResult = waterDepthRule.Validate(textBoxWaterDepthRule.Text, CultureInfo.CurrentCulture);
                    this.Tag = textBoxWaterDepthRule.Text;
                    break;

                default:
                    break;
            }

            if (validationResult == ValidationResult.ValidResult)
            {
                this.DialogResult = true;
                this.Close();
            }

            
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = null;
            this.Close();
        }

        /// <summary>
        /// Selects the text in the textbox when textbox gets focus.
        /// So, when tabbing to the textbox you are ready to write the new text!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectText(object sender, RoutedEventArgs e)
        {
            TextBox tb = (sender as TextBox);

            if (tb != null)
            {
                tb.SelectAll();
            }
        }

        /// <summary>
        /// Selects text in textbox when clicking mouse in the textbox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectivelyIgnoreMouseButton(object sender, MouseButtonEventArgs e)
        {
            TextBox tb = (sender as TextBox);

            if (tb != null)
            {
                if (!tb.IsKeyboardFocusWithin)
                {
                    e.Handled = true;
                    tb.Focus();
                }
            }
        }

        /// <summary>
        /// Workaround to force validation error text to appear after pressing tab key
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxSpeedRule_LostFocus(object sender, RoutedEventArgs e)
        {
            // Force validation
            textBoxSpeedRule.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }

        /// <summary>
        /// Workaround to force validation error text to appear after pressing tab key
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxWaterDepthRule_LostFocus(object sender, RoutedEventArgs e)
        {
            // Force validation
            textBoxWaterDepthRule.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }

    }
}
