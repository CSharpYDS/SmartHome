﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Edge.Database.Models;
using Edge.DTO;

namespace Edge.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TemperatureController : ControllerBase
    {
        private readonly Context context;

        public TemperatureController(Context context)
        {
            this.context = context;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return StatusCode(404);
        }

        [HttpPost("{value}")]
        public IActionResult Post(double value)
        {
            context.Temperature.Add(new Temperature() { Timer = DateTime.Now, Val = value });
            context.SaveChanges();

            Settings settings = context.Settings.First();
            PostTemperatureOutputDTO dto = new PostTemperatureOutputDTO() { 
                clim_on = value >= settings.ClimOn /*&& value >= settings.ClimOff*/,
                heat_on = value <= settings.HeatOn /*&& value <= settings.HeatOff*/
            };

            Stattemperature stats = context.Stattemperature.Find(DateTime.Now.ToString("yyyy"));
            if (stats == null)
            {
                stats = new Stattemperature() { Statyear = DateTime.Now.ToString("yyyy") };
                context.Add(stats);
            }
            IQueryable<Temperature> query = context.Temperature.Where(s => s.Timer.Year == DateTime.Now.Year);
            stats.Average = query.Average(s => s.Val);
            stats.StdDeviation = query.Sum(s => s.Val - stats.Average) / query.Count();
            context.SaveChanges();

            return Ok(dto);
        }
    }
}
