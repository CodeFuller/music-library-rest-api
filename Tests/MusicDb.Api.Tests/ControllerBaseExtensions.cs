using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;

namespace MusicDb.Api.Tests
{
	public static class ControllerBaseExtensions
	{
		public static void StubControllerContext(this ControllerBase controller)
		{
			var httpContextStub = new Mock<HttpContext>();
			httpContextStub.Setup(x => x.Request).Returns(Mock.Of<HttpRequest>());

			controller.ControllerContext.HttpContext = httpContextStub.Object;

			var urlHelperStub = new Mock<IUrlHelper>();
			urlHelperStub.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns("/SomeUri");
			controller.Url = urlHelperStub.Object;
		}
	}
}
