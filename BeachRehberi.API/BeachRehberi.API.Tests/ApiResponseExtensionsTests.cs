using BeachRehberi.API.Extensions;
using BeachRehberi.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace BeachRehberi.API.Tests;

public class ApiResponseExtensionsTests
{
    [Fact]
    public void ToActionResult_maps_payment_disabled_to_503()
    {
        var result = ServiceResult<object>.Failure("kapalı", 503);
        var action = result.ToActionResult();
        var obj = Assert.IsType<ObjectResult>(action);
        Assert.Equal(503, obj.StatusCode);
    }

    [Fact]
    public void ToActionResult_maps_stripe_not_ready_to_501()
    {
        var result = ServiceResult<object>.Failure("henüz hazır değil", 501);
        var action = result.ToActionResult();
        var obj = Assert.IsType<ObjectResult>(action);
        Assert.Equal(501, obj.StatusCode);
    }
}
