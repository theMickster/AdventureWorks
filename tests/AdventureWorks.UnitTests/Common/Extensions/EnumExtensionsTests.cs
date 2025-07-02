using AdventureWorks.Common.Extensions;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AdventureWorks.UnitTests.Common.Extensions;

[ExcludeFromCodeCoverage]
public sealed class EnumExtensionsTests : UnitTestBase
{
        [Fact]
        public void Enum_extension_get_description_returns_value_when_no_description()
        {
            Months.January.GetDescription().Should().Be("January");
            Months.February.GetDescription().Should().Be("February");
            Months.March.GetDescription().Should().Be("March");
            Months.April.GetDescription().Should().Be("April");
            Months.May.GetDescription().Should().Be("May");
            Months.June.GetDescription().Should().Be("June");
            Months.July.GetDescription().Should().Be("July");
            Months.August.GetDescription().Should().Be("August");
            Months.September.GetDescription().Should().Be("September");
            Months.October.GetDescription().Should().Be("October");
            Months.November.GetDescription().Should().Be("November");
            Months.December.GetDescription().Should().Be("December");
        }

        [Fact]
        public void Enum_extension_get_description_returns_correct_description()
        {
            WeekDays.Sun.GetDescription().Should().Be("Sunday");
            WeekDays.Mon.GetDescription().Should().Be("Monday");
            WeekDays.Tue.GetDescription().Should().Be("Tuesday");
            WeekDays.Wed.GetDescription().Should().Be("Wednesday");
            WeekDays.Thu.GetDescription().Should().Be("Thursday");
            WeekDays.Fri.GetDescription().Should().Be("Friday");
            WeekDays.Sat.GetDescription().Should().Be("Saturday");
        }

        [Fact]
        public void Enum_extension_get_display_returns_correct_description()
        {
            WeekDays.Sun.GetDisplayName().Should().Be("Sunday");
            WeekDays.Mon.GetDisplayName().Should().Be("Monday");
            WeekDays.Tue.GetDisplayName().Should().Be("Tuesday");
            WeekDays.Wed.GetDisplayName().Should().Be("Wednesday");
            WeekDays.Thu.GetDisplayName().Should().Be("Thursday");
            WeekDays.Fri.GetDisplayName().Should().Be("Friday");
            WeekDays.Sat.GetDisplayName().Should().Be("Saturday");
        }


        private enum WeekDays
        {
            [Description("Sunday")]
            [Display(Name = "Sunday")]
            Sun,
            [Description("Monday")]
            [Display(Name = "Monday")]
            Mon,
            [Description("Tuesday")]
            [Display(Name = "Tuesday")]
            Tue,
            [Description("Wednesday")]
            [Display(Name = "Wednesday")]
            Wed,
            [Description("Thursday")]
            [Display(Name = "Thursday")]
            Thu,
            [Description("Friday")]
            [Display(Name = "Friday")]
            Fri,
            [Description("Saturday")]
            [Display(Name = "Saturday")]
            Sat
        }

        private enum Months
        {
            January,
            February,
            March,
            April,
            May,
            June,
            July,
            August,
            September,
            October,
            November,
            December
        }
}
