using ECommerce.Application.DTOs.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ECommerce.API.Filters;

public class ApiResponseWrapperFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        switch (context.Result)
        {
            // ✅ أي رد فيه Body (Ok, BadRequest, NotFound مع رسالة...)
            case ObjectResult objectResult:
                {
                    var statusCode = objectResult.StatusCode ?? StatusCodes.Status200OK;
                    var success = statusCode < 400;

                    object? data = objectResult.Value;
                    string? message = null;

                    // ✅ التعامل مع أخطاء FluentValidation التلقائية (شكلها مختلف عن باقي الردود)
                    if (objectResult.Value is ValidationProblemDetails validationProblem)
                    {
                        data = validationProblem.Errors;
                        message = "بيانات غير صحيحة";
                    }
                    // ✅ التعامل مع كل الأماكن اللي بترجع BadRequest("رسالة نصية") في الكود الحالي
                    else if (objectResult.Value is string textMessage)
                    {
                        data = null;
                        message = textMessage;
                    }

                    context.Result = new ObjectResult(new ApiResponse
                    {
                        Success = success,
                        Data = success ? data : null,
                        Message = message
                    })
                    {
                        StatusCode = statusCode
                    };
                    break;
                }

            // ✅ أي رد من غير Body زي NotFound() أو BadRequest() من غير رسالة
            case StatusCodeResult statusCodeResult when statusCodeResult.StatusCode != StatusCodes.Status204NoContent:
                {
                    var success = statusCodeResult.StatusCode < 400;

                    context.Result = new ObjectResult(new ApiResponse
                    {
                        Success = success,
                        Data = null,
                        Message = null
                    })
                    {
                        StatusCode = statusCodeResult.StatusCode
                    };
                    break;
                }

                // ℹ️ 204 No Content سيبناها زي ما هي عمدًا - القياسي HTTP إنها من غير Body خالص،
                // ومعناها واضح للـ Client أصلاً (نجاح، مفيش بيانات) من غير ما نحتاج نلف حولها.
        }

        await next();
    }
}