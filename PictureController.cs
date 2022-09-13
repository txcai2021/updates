using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Transactions;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Microsoft.EntityFrameworkCore;


namespace SIMTech.APS.Picture.API.Controllers
{
    using SIMTech.APS.Picture.API.Repository;
    using SIMTech.APS.Picture.API.Mappers;
    using SIMTech.APS.Picture.API.Models;
    using SIMTech.APS.PresentationModels;


    [Route("api/[controller]")]
    [ApiController]
    public class PictureController : ControllerBase
    {
        private readonly IPictureRepository _pictureRepository;
        private readonly ExceptionManager _exceptionManager;


        public PictureController(IPictureRepository PictureRepository)
        {
            _pictureRepository = PictureRepository;
            _exceptionManager = new ExceptionManager();
        }


        [HttpGet]
        public IEnumerable<PicturePM> GetAllPictures()
        {
            var pictures = _pictureRepository.GetQuery(x => x.Id > 0).ToList();

            return PictureMapper.ToPresentationModels(pictures).AsQueryable();

        }

        [HttpGet]
        [Route("{id}")]
        public PicturePM GetPictureById(int id) 
        {
            Picture picture = _pictureRepository.GetById(id);
            PicturePM picturePM = PictureMapper.ToPresentationModel(picture);

            return picturePM;
        }

        [HttpGet("Ids/{pictureIds}")]      
        public async Task<ActionResult<IEnumerable<PicturePM>>> GetPicturebyIds(string pictureIds)
        {
            var picIds = new List<int>();
            var pictures = new List<Picture>();

            if (pictureIds == "0")
            {
                 pictures = (await _pictureRepository.GetQueryAsync(x => x.Id > 0)).ToList();

            }
            else
            {
                try
                {
                    picIds = pictureIds.Split(",").Select(x => Int32.Parse(x)).ToList();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                pictures = (await _pictureRepository.GetQueryAsync(x =>  picIds.Contains(x.Id))).ToList();
               
            }

            return Ok(PictureMapper.ToPresentationModels(pictures).ToList());


            

        }


        [HttpPost]
        public int AddPicture([FromBody] PicturePM picturePM)
        {
            if (picturePM.PictureID == -100)
            {
                //SaveDataToFile(picturePM);
                return 0;
            }

            var picture = PictureMapper.FromPresentationModel(picturePM);


            _exceptionManager.Process(() =>
            {
                _pictureRepository.Insert(picture);
            }, "ExceptionShielding");


            return picture.Id;
           
        }

        [HttpPut]
        public void UpdatePicture([FromBody] PicturePM picturePM)
        {
            var picture = _pictureRepository.GetById(picturePM.PictureID);

            PictureMapper.UpdateEntity(picturePM, picture);


            _exceptionManager.Process(() =>
            {
                _pictureRepository.Update(picture);              
            }, "ExceptionShielding");
        }
       


    }
}
