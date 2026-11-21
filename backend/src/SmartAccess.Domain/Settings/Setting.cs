using SmartAccess.Domain.Common;

namespace SmartAccess.Domain.Settings;

/// <summary>CLAUDE.md 4.17: kalit — qiymat (jsonb). Yozuv bo'lmasa standart qiymat amal qiladi.</summary>
public sealed class Setting : TenantEntity
{
    public required string Key { get; set; }
    public required string Value { get; set; }
}

/// <summary>CLAUDE.md 4.17 jadvali — kalitlar va standart qiymatlar.</summary>
public static class SettingKeys
{
    public const string GraceMinutes = "attendance.grace_minutes";
    public const string OvertimeThresholdMinutes = "attendance.overtime_threshold_minutes";
    public const string WindowBeforeHours = "attendance.window_before_hours";
    public const string WindowAfterHours = "attendance.window_after_hours";
    public const string FaceConfidenceThreshold = "face.confidence_threshold";
    public const string FaceOperatorConfirmRequired = "face.operator_confirm_required";
    public const string FaceUnknownRetentionDays = "face.unknown_retention_days";
    public const string AnprConfidenceThreshold = "anpr.confidence_threshold";
    public const string AnprCountsForAttendance = "anpr.counts_for_attendance";
    public const string GateCommandTimeoutSeconds = "gate.command_timeout_seconds";
    public const string GateAutoOpenEnabled = "gate.auto_open_enabled";
    public const string VideoRetentionDays = "video.retention_days";
    public const string VideoMaxMainStreamsPerClient = "video.max_main_streams_per_client";
    public const string SecurityRequire2faForAdmin = "security.require_2fa_for_admin";
    public const string SecurityLockoutAttempts = "security.lockout_attempts";

    public static readonly IReadOnlyDictionary<string, string> Defaults = new Dictionary<string, string>
    {
        [GraceMinutes] = "10",
        [OvertimeThresholdMinutes] = "30",
        [WindowBeforeHours] = "4",
        [WindowAfterHours] = "6",
        [FaceConfidenceThreshold] = "85",
        [FaceOperatorConfirmRequired] = "true",
        [FaceUnknownRetentionDays] = "30",
        [AnprConfidenceThreshold] = "90",
        [AnprCountsForAttendance] = "false",
        [GateCommandTimeoutSeconds] = "3",
        [GateAutoOpenEnabled] = "false",
        [VideoRetentionDays] = "30",
        [VideoMaxMainStreamsPerClient] = "4",
        [SecurityRequire2faForAdmin] = "true",
        [SecurityLockoutAttempts] = "5",
    };
}
