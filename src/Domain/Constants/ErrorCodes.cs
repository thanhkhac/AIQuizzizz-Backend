// ReSharper disable InconsistentNaming
namespace CleanArchitectureBase.Domain.Constants;

public static class ErrorCodes
{
    //COMMON
    public const string COMMON_FORBIDDEN = nameof(COMMON_FORBIDDEN);
    public const string COMMON_SERVER_INTERNAL_ERROR = nameof(COMMON_SERVER_INTERNAL_ERROR);
    public const string COMMON_UNHANDLED_ERROR = nameof(COMMON_UNHANDLED_ERROR);
    public const string COMMON_NOT_FOUND = nameof(COMMON_NOT_FOUND);
    public const string COMMON_GONE = nameof(COMMON_GONE);
    public const string COMMON_BAD_REQUEST = nameof(COMMON_BAD_REQUEST);
    public const string COMMON_CONFLICT = nameof(COMMON_CONFLICT);
    public const string COMMON_TIMEOUT_ERROR = nameof(COMMON_TIMEOUT_ERROR);
    public const string FIELD_NAME_NOT_FOUND = nameof(FIELD_NAME_NOT_FOUND);

    //ACCOUNT
    public const string ACCOUNT_NOTFOUND = nameof(ACCOUNT_NOTFOUND);
    public const string ACCOUNT_LOCKED_OUT = nameof(ACCOUNT_LOCKED_OUT);
    public const string ACCOUNT_BANNED = nameof(ACCOUNT_BANNED);
    public const string ACCOUNT_USERNAME_ALREADY_EXISTS = nameof(ACCOUNT_USERNAME_ALREADY_EXISTS);
    public const string ACCOUNT_EMAIL_ALREADY_EXISTS = nameof(ACCOUNT_EMAIL_ALREADY_EXISTS);
    public const string ACCOUNT_INVALID_PASSWORD = nameof(ACCOUNT_INVALID_PASSWORD);
    public const string ACCOUNT_INVALID_CREDENTIALS = nameof(ACCOUNT_INVALID_CREDENTIALS);
    public const string ACCOUNT_INVALID_VERIFICATION_CODE = nameof(ACCOUNT_INVALID_VERIFICATION_CODE);
    public const string ACCOUNT_EMAIL_NOT_VERIFIED = nameof(ACCOUNT_EMAIL_NOT_VERIFIED); 
    public const string ACCOUNT_EMAIL_BANNED  = nameof(ACCOUNT_EMAIL_BANNED ); 
    public const string ACCOUNT_INVALID_RESET_CODE  = nameof(ACCOUNT_INVALID_RESET_CODE ); 
    public const string ACCOUNT_WRONG_PASSWORD  = nameof(ACCOUNT_WRONG_PASSWORD ); 
    // public const string ACCOUNT_TOO_MANY_REQUESTS  = nameof(ACCOUNT_TOO_MANY_REQUESTS ); 
    public const string EMAIL_VERIFICATION_REQUEST_TOO_MANY  = nameof(EMAIL_VERIFICATION_REQUEST_TOO_MANY ); 
    public const string EMAIL_VERIFICATION_CODE_FAILED_TOO_MANY  = nameof(EMAIL_VERIFICATION_CODE_FAILED_TOO_MANY ); 
    public const string PASSWORD_RESET_REQUEST_TOO_MANY  = nameof(PASSWORD_RESET_REQUEST_TOO_MANY ); 
    public const string PASSWORD_RESET_CODE_FAILED_TOO_MANY  = nameof(PASSWORD_RESET_CODE_FAILED_TOO_MANY ); 
    
    //IDENTITY OVERRIDE ERROR DESCRIBER
    public const string IDENTITY_DEFAULT_ERROR = nameof(IDENTITY_DEFAULT_ERROR);
    public const string IDENTITY_CONCURRENCY_FAILURE = nameof(IDENTITY_CONCURRENCY_FAILURE);
    public const string IDENTITY_PASSWORD_MISMATCH = nameof(IDENTITY_PASSWORD_MISMATCH);
    public const string IDENTITY_INVALID_TOKEN = nameof(IDENTITY_INVALID_TOKEN);
    public const string IDENTITY_RECOVERY_CODE_REDEMPTION_FAILED = nameof(IDENTITY_RECOVERY_CODE_REDEMPTION_FAILED);
    public const string IDENTITY_LOGIN_ALREADY_ASSOCIATED = nameof(IDENTITY_LOGIN_ALREADY_ASSOCIATED);
    public const string IDENTITY_INVALID_USER_NAME = nameof(IDENTITY_INVALID_USER_NAME);
    public const string IDENTITY_INVALID_EMAIL = nameof(IDENTITY_INVALID_EMAIL);
    public const string IDENTITY_DUPLICATE_USER_NAME = nameof(IDENTITY_DUPLICATE_USER_NAME);
    public const string IDENTITY_DUPLICATE_EMAIL = nameof(IDENTITY_DUPLICATE_EMAIL);
    public const string IDENTITY_INVALID_ROLE_NAME = nameof(IDENTITY_INVALID_ROLE_NAME);
    public const string IDENTITY_DUPLICATE_ROLE_NAME = nameof(IDENTITY_DUPLICATE_ROLE_NAME);
    public const string IDENTITY_USER_ALREADY_HAS_PASSWORD = nameof(IDENTITY_USER_ALREADY_HAS_PASSWORD);
    public const string IDENTITY_USER_LOCKOUT_NOT_ENABLED = nameof(IDENTITY_USER_LOCKOUT_NOT_ENABLED);
    public const string IDENTITY_USER_ALREADY_IN_ROLE = nameof(IDENTITY_USER_ALREADY_IN_ROLE);
    public const string IDENTITY_USER_NOT_IN_ROLE = nameof(IDENTITY_USER_NOT_IN_ROLE);
    public const string IDENTITY_PASSWORD_TOO_SHORT = nameof(IDENTITY_PASSWORD_TOO_SHORT);
    public const string IDENTITY_PASSWORD_REQUIRES_UNIQUE_CHARS = nameof(IDENTITY_PASSWORD_REQUIRES_UNIQUE_CHARS);
    public const string IDENTITY_PASSWORD_REQUIRES_NON_ALPHANUMERIC = nameof(IDENTITY_PASSWORD_REQUIRES_NON_ALPHANUMERIC);
    public const string IDENTITY_PASSWORD_REQUIRES_DIGIT = nameof(IDENTITY_PASSWORD_REQUIRES_DIGIT);
    public const string IDENTITY_PASSWORD_REQUIRES_LOWER = nameof(IDENTITY_PASSWORD_REQUIRES_LOWER);
    public const string IDENTITY_PASSWORD_REQUIRES_UPPER = nameof(IDENTITY_PASSWORD_REQUIRES_UPPER);
    
    //Class
    public const string CLASS_NOTFOUND = nameof(CLASS_NOTFOUND);
    public const string CLASS_ALREADY_EXISTS = nameof(CLASS_ALREADY_EXISTS);
    public const string CLASS_CODE_NOT_FOUND = nameof(CLASS_CODE_NOT_FOUND);
    public const string STUDENT_ALREADY_EXISTS_IN_CLASS = nameof(STUDENT_ALREADY_EXISTS_IN_CLASS);
    public const string ONLY_OWNERS_CAN_UPDATE = nameof(ONLY_OWNERS_CAN_UPDATE);
    public const string NOT_FOUND_STUDENT_IN_CLASS = nameof(NOT_FOUND_STUDENT_IN_CLASS);
    public const string NOT_FOUND_TEACHER_OR_OWNER_IN_CLASS = nameof(NOT_FOUND_TEACHER_OR_OWNER_IN_CLASS);
    public const string NOT_FOUND_USER_IN_CLASS = nameof(NOT_FOUND_USER_IN_CLASS);
    public const string QUESTION_SET_ALREADY_IN_CLASS = nameof(QUESTION_SET_ALREADY_IN_CLASS);
    public const string NOT_HAVE_PERMISSION_TO_ADD_QUESTION_SET = nameof(NOT_HAVE_PERMISSION_TO_ADD_QUESTION_SET);
    
    //Question
    public const string INVALID_QUESTION_TYPE = nameof(INVALID_QUESTION_TYPE);
    
    //Question set
    public const string QUESTION_SET_NOT_FOUND = nameof(QUESTION_SET_NOT_FOUND);
    public const string QUESTION_SET_NOT_FOUND_IN_CLASS = nameof(QUESTION_SET_NOT_FOUND_IN_CLASS);
    public const string USER_NOT_ACCESS_TO_QUESTION_SET = nameof(USER_NOT_ACCESS_TO_QUESTION_SET);
    
    public const string PLAN_REQUIRE_PLAN = nameof(PLAN_REQUIRE_PLAN);
    public const string PLAN_NOT_FOUND = nameof(PLAN_NOT_FOUND);
    
    //FOLDER
    public const string FOLDER_NOT_FOUND = nameof(FOLDER_NOT_FOUND);
    public const string FOLDER_ALREADY_EXISTS = nameof(FOLDER_ALREADY_EXISTS);
    public const string USER_NOT_HAVE_PERMISSION_IN_FOLDER = nameof(USER_NOT_HAVE_PERMISSION_IN_FOLDER);
    
    //Test
    public const string TEST_TEMPLATE_NOT_FOUND = nameof(TEST_TEMPLATE_NOT_FOUND);
    public const string MAX_ATTEMPT_IN_THIS_TEST = nameof(MAX_ATTEMPT_IN_THIS_TEST);
    public const string TEST_IS_OVERDUE = nameof(TEST_IS_OVERDUE);
    public const string TEST_NOT_FOUND = nameof(TEST_NOT_FOUND);
    public const string NUMBER_OF_QUESTION_EXCEED_LIMIT = nameof(NUMBER_OF_QUESTION_EXCEED_LIMIT);
    public const string USER_NOT_HAVE_PERMISSION_IN_TEST_TEMPLATE = nameof(USER_NOT_HAVE_PERMISSION_IN_TEST_TEMPLATE);
    
    //File
    public const string FILE_NOT_FOUND = nameof(FILE_NOT_FOUND);
    public const string INVALID_FILE_FORMAT = nameof(INVALID_FILE_FORMAT);
    public const string ERROR_FORMAT_FILE = nameof(ERROR_FORMAT_FILE);
    
}
