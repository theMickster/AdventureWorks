namespace AdventureWorks.Common.Constants;

/// <summary>
/// Constants for Human Resources domain business rules and defaults.
/// </summary>
public static class HumanResourcesConstants
{
    /// <summary>
    /// Default vacation hours granted to new hires.
    /// </summary>
    public const short DefaultVacationHours = 40;

    /// <summary>
    /// Default sick leave hours granted to new hires.
    /// </summary>
    public const short DefaultSickLeaveHours = 24;

    /// <summary>
    /// Minimum number of days that must pass between termination and rehire eligibility.
    /// </summary>
    public const int MinimumDaysBeforeRehire = 90;

    /// <summary>
    /// Maximum vacation hours that can be granted (business rule limit).
    /// </summary>
    public const short MaximumVacationHours = 240;

    /// <summary>
    /// Maximum sick leave hours that can be granted (business rule limit).
    /// </summary>
    public const short MaximumSickLeaveHours = 480;

    /// <summary>
    /// Maximum pay rate allowed (hourly or annual depending on SalariedFlag).
    /// </summary>
    public const decimal MaximumPayRate = 500.00m;

    /// <summary>
    /// Maximum days in the future a hire date can be scheduled.
    /// </summary>
    public const int MaximumFutureHireDays = 30;

    /// <summary>
    /// Maximum days in the future a termination date can be scheduled.
    /// </summary>
    public const int MaximumFutureTerminationDays = 90;

    /// <summary>
    /// Minimum shift ID (1 = Day shift).
    /// </summary>
    public const byte MinimumShiftId = 1;

    /// <summary>
    /// Maximum shift ID (3 = Night shift).
    /// </summary>
    public const byte MaximumShiftId = 3;

    /// <summary>
    /// Monthly pay frequency code.
    /// </summary>
    public const byte PayFrequencyMonthly = 1;

    /// <summary>
    /// Bi-weekly pay frequency code.
    /// </summary>
    public const byte PayFrequencyBiWeekly = 2;

    /// <summary>
    /// Temporary hire date placeholder for newly created employees.
    /// Employees must go through the hire lifecycle workflow to set their actual hire date.
    /// </summary>
    public static readonly DateTime TemporaryHireDate = new DateTime(1901, 1, 1);
}
