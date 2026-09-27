namespace EduCore.Application.Common;

public static class ErrorCodes
{
    public static class Enrollment
    {

        // for EnrollmentService
        public const string NotFound = "ENROLLMENT_NOT_FOUND"; 
        public const string NotActive = "ENROLLMENT_NOT_ACTIVE";

        // for fluent validation
        public const string StudentIdInvalid = "ENROLLMENT_STUDENT_ID_INVALID";
        public const string GroupIdInvalid = "ENROLLMENT_GROUP_ID_INVALID";

    }

    public static class Group
    {
        // for GroupService
        public const string NotFound = "GROUP_NOT_FOUND";
        public const string GroupNoTeacher = "GROUP_NO_TEACHER";
        public const string NotActive = "GROUP_NOT_ACTIVE";

        // for fluent validation
        public const string NameRequired = "GROUP_NAME_REQUIRED";
        public const string NameTooLong = "GROUP_NAME_TOO_LONG";
        public const string TeacherIdInvalid = "GROUP_TEACHER_ID_INVALID";
        public const string StandardFeeInvalid = "GROUP_STANDARD_FEE_INVALID";
    }

    public static class Attendance
    {
        // for AttendanceService
        public const string NotFound = "ATTENDANCE_NOT_FOUND";        
        public const string AlreadyExists = "ATTENDANCE_ALREADY_EXISTS";

        // for fluent validation
        public const string InvalidEnrollmentId = "ATTENDANCE_INVALID_ENROLLMENT_ID";
        public const string DateRequired = "ATTENDANCE_DATE_REQUIRED";
        public const string DateInFuture = "ATTENDANCE_DATE_IN_FUTURE";
        public const string InvalidStatus = "ATTENDANCE_INVALID_STATUS";
        public const string NotesTooLong = "ATTENDANCE_NOTES_TOO_LONG";
        public const string DateTooOld = "ATTENDANCE_DATE_TOO_OLD";
    }

    public static class Student
    {
        // for StudentService
        public const string NotFound = "STUDENT_NOT_FOUND";
        public const string StudentAlreadyEnrolled = "STUDENT_ALREADY_ENROLLED";
        public const string AlreadyExists = "STUDENT_ALREADY_EXISTS";
        public const string NotActive = "STUDENT_NOT_ACTIVE";

        // for fluent validation
        public const string FirstNameRequired = "STUDENT_FIRST_NAME_REQUIRED";
        public const string FirstNameTooLong = "STUDENT_FIRST_NAME_TOO_LONG";
        public const string LastNameRequired = "STUDENT_LAST_NAME_REQUIRED";
        public const string LastNameTooLong = "STUDENT_LAST_NAME_TOO_LONG";
        public const string GuardianNameTooLong = "STUDENT_GUARDIAN_NAME_TOO_LONG";
        public const string GuardianPhoneInvalid = "STUDENT_GUARDIAN_PHONE_INVALID";
        public const string BirthDateRequired = "STUDENT_BIRTHDATE_REQUIRED";
        public const string BirthDateInFuture = "STUDENT_BIRTHDATE_IN_FUTURE";
        public const string BirthDateInvalid = "STUDENT_BIRTHDATE_INVALID";
        public const string GenderRequired = "STUDENT_GENDER_REQUIRED";
        public const string GenderInvalid = "STUDENT_GENDER_INVALID";
        public const string EmailTooLong = "STUDENT_EMAIL_TOO_LONG";
        public const string EmailInvalid = "STUDENT_EMAIL_INVALID";


    }

    public static class Teacher
    {
        // for TeacherService
        public const string NotFound = "TEACHER_NOT_FOUND";
        public const string NotActive = "TEACHER_NOT_ACTIVE";

        // for fluent validation
        public const string FirstNameRequired = "TEACHER_FIRST_NAME_REQUIRED";
        public const string FirstNameTooLong = "TEACHER_FIRST_NAME_TOO_LONG";
        public const string LastNameRequired = "TEACHER_LAST_NAME_REQUIRED";
        public const string LastNameTooLong = "TEACHER_LAST_NAME_TOO_LONG";
        public const string PhoneNumberRequired = "TEACHER_PHONE_NUMBER_REQUIRED";
        public const string PhoneNumberTooLong = "TEACHER_PHONE_NUMBER_TOO_LONG";
        public const string EmailRequired = "TEACHER_EMAIL_REQUIRED";
        public const string EmailTooLong = "TEACHER_EMAIL_TOO_LONG";
        public const string EmailInvalid = "TEACHER_EMAIL_INVALID";
        public const string PhoneNumberInvalid = "TEACHER_PHONE_NUMBER_INVALID";
        public const string SpecializationTooLong = "TEACHER_SPECIALIZATION_TOO_LONG";
        public const string HireDateInvalid = "TEACHER_HIRE_DATE_INVALID";


    }

    public static class User
    {
        // for UserService
        public const string NotFound = "USER_NOT_FOUND";
        public const string NotActive = "USER_NOT_ACTIVE";
        public const string AlreadyExists = "USER_ALREADY_EXISTS";
        public const string InvalidCredentials = "INVALID_CREDENTIALS";
        public const string MustBeLoggedIn = "MUST_BE_LOGGED_IN";
        public const string Forbidden = "FORBIDDEN";
        public const string Unauthorized = "UNAUTHORIZED";
        public const string InvalidUserNamePassword = "INVALID_USERNAME_OR_PASSWORD";

        // for fluent validation
        public const string UserNameRequired = "USER_USERNAME_REQUIRED";
        public const string UserNameTooLong = "USER_USERNAME_TOO_LONG";
        public const string PasswordRequired = "USER_PASSWORD_REQUIRED";
        public const string PasswordTooShort = "USER_PASSWORD_TOO_SHORT";
        public const string PasswordTooLong = "USER_PASSWORD_TOO_LONG";
        public const string CurrentPasswordRequired = "USER_Current_PASSWORD_REQUIRED";
        public const string CurrentPasswordTooShort = "USER_Current_PASSWORD_TOO_SHORT";
        public const string CurrentPasswordTooLong = "USER_Current_PASSWORD_TOO_LONG";
        public const string NewPasswordRequired = "USER_NEW_PASSWORD_REQUIRED";
        public const string NewPasswordTooShort = "USER_NEW_PASSWORD_TOO_SHORT";
        public const string NewPasswordTooLong = "USER_NEW_PASSWORD_TOO_LONG";
        public const string ConfirmNewPasswordRequired = "USER_CONFIRM_NEW_PASSWORD_REQUIRED";
        public const string ConfirmNewPasswordTooShort = "USER_CONFIRM_NEW_PASSWORD_TOO_SHORT";
        public const string ConfirmNewPasswordTooLong = "USER_CONFIRM_NEW_PASSWORD_TOO_LONG";
        public const string NewPasswordMismatch = "USER_CONFIRM_NEW_PASSWORD_DOES_NOT_MATCH";
        public const string FirstNameRequired = "USER_FIRST_NAME_REQUIRED";
        public const string FirstNameTooLong = "USER_FIRST_NAME_TOO_LONG";
        public const string LastNameRequired = "USER_LAST_NAME_REQUIRED";
        public const string LastNameTooLong = "USER_LAST_NAME_TOO_LONG";
        public const string PhoneNumberRequired = "USER_PHONE_NUMBER_REQUIRED";
        public const string PhoneNumberTooLong = "USER_PHONE_NUMBER_TOO_LONG";
        public const string EmailRequired = "USER_EMAIL_REQUIRED";
        public const string EmailTooLong = "USER_EMAIL_TOO_LONG";
        public const string EmailInvalid = "USER_EMAIL_INVALID";
        public const string PhoneNumberInvalid = "USER_PHONE_NUMBER_INVALID";

    }

    public static class StudentFee 
    {
        // for StudentFeeService
        public const string NotFound = "STUDENT_FEE_NOT_FOUND";
        public const string CannotChangeAmountWithPayments = "STUDENT_FEE_CANNOT_CHANGE_AMOUNT_WITH_PAYMENTS";
        public const string CannotDeleteWithPayments = "CANNOT_DELETE_THE_STUDENT_FEE_BECAUSE_IT_HAS_ASSOCIATED_PAYMENTS";

        // for fluent validation
        public const string EnrollmentIdInvalid = "STUDENT_FEE_ENROLLMENT_ID_INVALID";
        public const string TitleRequired = "STUDENT_FEE_TITLE_REQUIRED";
        public const string TitleTooLong = "STUDENT_FEE_TITLE_TOO_LONG";
        public const string TotalAmountInvalid = "STUDENT_FEE_TOTAL_AMOUNT_INVALID";
        public const string DueDateInvalid = "STUDENT_FEE_DUE_DATE_INVALID";
        public const string DescriptionTooLong = "STUDENT_FEE_DESCRIPTION_TOO_LONG";

    }

    public static class Payment
    {
        // for PaymentService
        public const string NotFound = "PAYMENT_NOT_FOUND";
        public const string ExceedsRemainingAmount = "PAYMENT_AMOUNT_EXCEEDS_REMAINING";
        public const string CannotUpdateCancelled = "PAYMENT_CANNOT_UPDATE_CANCELLED";
        public const string InvalidAmount = "PAYMENT_AMOUNT_INVALID";

        // for fluent validation
        public const string StudentFeeIdInvalid = "PAYMENT_STUDENT_FEE_ID_INVALID";
        public const string AmountInvalid = "PAYMENT_AMOUNT_INVALID";
        public const string PaymentDateRequired = "PAYMENT_PAYMENT_DATE_REQUIRED";
        public const string PaymentMetodeRequired = "PAYMENT_PAYMENT_METHOD_REQUIRED";
        public const string PaymentNotesTooLong = "PAYMENT_PAYMENT_NOTES_TOO_LONG";
        public const string PaymentDateInvalid = "PAYMENT_PAYMENT_DATE_INVALID";


    }

    public static class StudentImport
    {
        // Global file-level errors 
        public const string EmptyFile = "STUDENT_IMPORT_EMPTY_FILE";
        public const string NoDataRows = "STUDENT_IMPORT_NO_DATA_ROWS";
        public const string DefaultPasswordNotConfigured = "STUDENT_IMPORT_DEFAULT_PASSWORD_NOT_CONFIGURED";

        // Row-level errors inside Excel (added to ReadErrors list)
        public const string NameRequired = "STUDENT_IMPORT_NAME_REQUIRED";
        public const string InvalidBirthDate = "STUDENT_IMPORT_INVALID_BIRTHDATE";
        public const string InvalidGender = "STUDENT_IMPORT_INVALID_GENDER";
        public const string AlreadyExists = "STUDENT_IMPORT_ALREADY_EXISTS";
    }

}