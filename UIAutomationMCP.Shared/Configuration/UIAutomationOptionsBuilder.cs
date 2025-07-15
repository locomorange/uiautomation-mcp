using Microsoft.Extensions.Configuration;
using UIAutomationMCP.Shared.Options;

namespace UIAutomationMCP.Shared.Configuration
{
    /// <summary>
    /// AOT-compatible UIAutomationOptions builder
    /// </summary>
    public static class UIAutomationOptionsBuilder
    {
        /// <summary>
        /// Build UIAutomationOptions from configuration
        /// </summary>
        public static UIAutomationOptions Build(IConfiguration configuration)
        {
            var options = new UIAutomationOptions();
            var section = configuration.GetSection(UIAutomationOptions.SectionName);
            
            if (!section.Exists())
                return options;
            
            // Bind all subsections
            BindElementSearchOptions(options.ElementSearch, section.GetSection(nameof(options.ElementSearch)));
            BindWindowOperationOptions(options.WindowOperation, section.GetSection(nameof(options.WindowOperation)));
            BindTextOperationOptions(options.TextOperation, section.GetSection(nameof(options.TextOperation)));
            BindTransformOptions(options.Transform, section.GetSection(nameof(options.Transform)));
            BindRangeValueOptions(options.RangeValue, section.GetSection(nameof(options.RangeValue)));
            BindLayoutOptions(options.Layout, section.GetSection(nameof(options.Layout)));
            
            return options;
        }
        
        private static void BindElementSearchOptions(ElementSearchOptions options, IConfigurationSection section)
        {
            section.Bind(options,
                (nameof(options.DefaultScope), ConfigurationHelper.CreateSetter<string>(v => options.DefaultScope = v)),
                (nameof(options.UseCache), ConfigurationHelper.CreateSetter<bool>(v => options.UseCache = v)),
                (nameof(options.UseRegex), ConfigurationHelper.CreateSetter<bool>(v => options.UseRegex = v)),
                (nameof(options.UseWildcard), ConfigurationHelper.CreateSetter<bool>(v => options.UseWildcard = v)),
                (nameof(options.MaxResults), ConfigurationHelper.CreateSetter<int>(v => options.MaxResults = v)),
                (nameof(options.ValidatePatterns), ConfigurationHelper.CreateSetter<bool>(v => options.ValidatePatterns = v))
            );
        }
        
        private static void BindWindowOperationOptions(WindowOperationOptions options, IConfigurationSection section)
        {
            section.Bind(options,
                (nameof(options.IncludeInvisible), ConfigurationHelper.CreateSetter<bool>(v => options.IncludeInvisible = v)),
                (nameof(options.DefaultAction), ConfigurationHelper.CreateSetter<string>(v => options.DefaultAction = v))
            );
        }
        
        private static void BindTextOperationOptions(TextOperationOptions options, IConfigurationSection section)
        {
            section.Bind(options,
                (nameof(options.DefaultIgnoreCase), ConfigurationHelper.CreateSetter<bool>(v => options.DefaultIgnoreCase = v)),
                (nameof(options.DefaultBackward), ConfigurationHelper.CreateSetter<bool>(v => options.DefaultBackward = v)),
                (nameof(options.DefaultTraverseUnit), ConfigurationHelper.CreateSetter<string>(v => options.DefaultTraverseUnit = v)),
                (nameof(options.DefaultTraverseCount), ConfigurationHelper.CreateSetter<int>(v => options.DefaultTraverseCount = v))
            );
        }
        
        private static void BindTransformOptions(TransformOptions options, IConfigurationSection section)
        {
            section.Bind(options,
                (nameof(options.DefaultX), ConfigurationHelper.CreateSetter<double>(v => options.DefaultX = v)),
                (nameof(options.DefaultY), ConfigurationHelper.CreateSetter<double>(v => options.DefaultY = v)),
                (nameof(options.DefaultWidth), ConfigurationHelper.CreateSetter<double>(v => options.DefaultWidth = v)),
                (nameof(options.DefaultHeight), ConfigurationHelper.CreateSetter<double>(v => options.DefaultHeight = v)),
                (nameof(options.DefaultRotationDegrees), ConfigurationHelper.CreateSetter<double>(v => options.DefaultRotationDegrees = v))
            );
        }
        
        private static void BindRangeValueOptions(RangeValueOptions options, IConfigurationSection section)
        {
            section.Bind(options,
                (nameof(options.DefaultValue), ConfigurationHelper.CreateSetter<double>(v => options.DefaultValue = v)),
                (nameof(options.DefaultMinimum), ConfigurationHelper.CreateSetter<double>(v => options.DefaultMinimum = v)),
                (nameof(options.DefaultMaximum), ConfigurationHelper.CreateSetter<double>(v => options.DefaultMaximum = v)),
                (nameof(options.DefaultStep), ConfigurationHelper.CreateSetter<double>(v => options.DefaultStep = v))
            );
        }
        
        private static void BindLayoutOptions(LayoutOptions options, IConfigurationSection section)
        {
            section.Bind(options,
                (nameof(options.DefaultDockPosition), ConfigurationHelper.CreateSetter<string>(v => options.DefaultDockPosition = v)),
                (nameof(options.DefaultExpandCollapseAction), ConfigurationHelper.CreateSetter<string>(v => options.DefaultExpandCollapseAction = v)),
                (nameof(options.DefaultScrollDirection), ConfigurationHelper.CreateSetter<string>(v => options.DefaultScrollDirection = v)),
                (nameof(options.DefaultScrollAmount), ConfigurationHelper.CreateSetter<double>(v => options.DefaultScrollAmount = v)),
                (nameof(options.DefaultHorizontalScrollPercent), ConfigurationHelper.CreateSetter<double>(v => options.DefaultHorizontalScrollPercent = v)),
                (nameof(options.DefaultVerticalScrollPercent), ConfigurationHelper.CreateSetter<double>(v => options.DefaultVerticalScrollPercent = v))
            );
        }
    }
}