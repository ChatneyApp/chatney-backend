package error_utils

type AppError struct {
	Code    string
	Message string
	Err     error
}

func MakeError(code string, message string, err error) AppError {
	return AppError{
		Code:    code,
		Message: message,
		Err:     err,
	}
}

func LogError(appError AppError) {
	if appError.Err != nil {
		println(appError.Code, appError.Message, appError.Err.Error())
	} else {
		println(appError.Code, appError.Message)
	}
}
