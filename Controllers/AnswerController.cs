using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using StackAlchemy_Back.Models;

[ApiController]
[Route("api/[controller]")]

public class AnswerController : ControllerBase
{
    private readonly AnswerRepository _AnswerRepository;
    private readonly TokenService _tokenService;

    public AnswerController(AnswerRepository AnswerRepository, TokenService tokenService)
    {
        _AnswerRepository = AnswerRepository;
        _tokenService = tokenService;
    }

    [HttpDelete("DeleteAnswer")]
    public ActionResult DeleteAnswer([FromBody] DeleteAnswerDTO deleteAnswerDTO)
    {
       if(string.IsNullOrWhiteSpace(deleteAnswerDTO.token))
       {
         return BadRequest(new{message = "invalid token."});
       }
       if(deleteAnswerDTO.AnswerId == null)
       {
         return BadRequest(new{message = "invalid answer id."});
       }

       var UserClaims = _tokenService.ValidateToken(deleteAnswerDTO.token);
       if(UserClaims == null)
       {
        return BadRequest(new {message = "error on validating token."});
       }

      var UserIdClaim = UserClaims?.FindFirst(ClaimTypes.NameIdentifier);

      if (UserIdClaim == null)
      {
        return BadRequest(new { message = "User ID not found in token." });
      }

      int userId;
      if (!int.TryParse(UserIdClaim.Value, out userId))
      {
        return BadRequest(new { message = "error on parsing user id to int." });
      }

      bool answerDelete =  _AnswerRepository.DeleteAnswer(userId,deleteAnswerDTO.AnswerId);
      if(answerDelete == false)
      {
        return BadRequest(new {message = "Cant Delete Answer."});
      }
       return Ok(new {message = "answer deleted succesfully."});
    }

    [HttpPost("CreateAnswer")]
    public IActionResult CreateAnswer([FromBody] CreateAnswerDTO createAnswerDTO)
    {
        var userPrincipal = _tokenService.ValidateToken(Request.Headers["Authorization"].ToString().Replace("Bearer ", ""));
        if (userPrincipal == null)
        {
            return Unauthorized(new { message = "Invalid or missing token." });
        }
        var userId = userPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "User ID is missing in the token." });
        }
        Answer CreatedAnswer = _AnswerRepository.CreateAnswer(int.Parse(userId), createAnswerDTO.QuestionId, createAnswerDTO.Title, createAnswerDTO.Code, createAnswerDTO.Description);
        if (CreatedAnswer == null)
        {
            return BadRequest();
        }
        return Ok(new { message = "Succesfully created an answer!", answer = CreatedAnswer });
    }

    [HttpGet("GetQuestionsAnswers")]
    public IActionResult GetQuestionsAnswers([FromHeader] int questionId)
    {
        List<Answer> answers = _AnswerRepository.GetQuestionsAnswers(questionId);
        return Ok(new { answers = answers, message = "succesfully returned answers" });
    }

    [HttpGet("GetAllAnswers")]
    public IActionResult GetAllAnswers()
    {
        return Ok(_AnswerRepository.GetAllAnswers());
    }

    [HttpGet("GetLikesAndDislikes")]
    public IActionResult GetLikesAndDislikes([FromBody] int answerId)
    {
        return Ok(_AnswerRepository.GetLikesAndDislikes(answerId));
    }



    [HttpPost("LikeAnswer")]
    public IActionResult Likeanswer([FromBody] int userId, int answerId)
    {
        bool liked = _AnswerRepository.LikeAnswer(userId, answerId);
        if (liked == false)
        {
            return BadRequest("Erron on liking the answer.");
        }

        return Ok("Liked an answer.");
    }

    [HttpPost("DislikeAnswer")]
    public IActionResult Dislikeanswer([FromBody] int userId, int answerId)
    {
        bool disliked = _AnswerRepository.DislikeAnswer(userId, answerId);
        if (disliked == false)
        {
            return BadRequest("Error on disliking the answer.");
        }

        return Ok("Disliked an answer.");
    }
}