using Microsoft.AspNetCore.Mvc;

namespace TrainService.Endpoints.Controllers;

[ApiController]
[Route("api/trains")]
public class TrainsController : ControllerBase
{
    public class AddTrainRequest
    {
        public string TrainNumber { get; set; } = string.Empty;
        public string TrainName { get; set; } = string.Empty;
        public string SourceStation { get; set; } = string.Empty;
        public string DestinationStation { get; set; } = string.Empty;
        public string DepartureTime { get; set; } = string.Empty;
        public string ArrivalTime { get; set; } = string.Empty;
    }

    [HttpPost]
    public ActionResult<object> Add([FromBody] AddTrainRequest input)
    {
        return Accepted(new { acknowledged = true, trainNumber = input.TrainNumber });
    }
}
