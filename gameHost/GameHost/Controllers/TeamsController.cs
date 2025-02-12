﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameHost.Models;

namespace GameHost.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeamsController : ControllerBase
    {
        private readonly GameHostDbContext _context;

        public TeamsController(GameHostDbContext context)
        {
            _context = context;
        }

        // GET: api/Teams
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Team>>> GetTeams()
        {
            if (_context.Teams == null)
            {
                return NotFound();
            }
            return await _context.Teams.ToListAsync();
        }

        // GET: api/Teams/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Team>> GetTeam(int id)
        {
            if (_context.Teams == null)
            {
                return NotFound();
            }
            var team = await _context.Teams.FindAsync(id);

            if (team == null)
            {
                return NotFound();
            }

            return team;
        }

        // PUT: api/Teams/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTeam(int id, Team team)
        {
            if (id != team.TeamId)
            {
                return BadRequest();
            }

            _context.Entry(team).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TeamExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Teams
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Team>> PostTeam(Team team)
        {
            if (_context.Teams == null)
            {
                return Problem("Entity set 'GameHostDbContext.Teams'  is null.");
            }
            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTeam", new { id = team.TeamId }, team);
        }

        [HttpPost("team/{maxTeams}")]
        public async Task<ActionResult<Team>> PostTeamWithCondition(Team team, int maxTeams)
        {
            if (_context.Teams == null)
            {
                return Problem("Entity set 'GameHostDbContext.Teams'  is null.");
            }
            bool teamNameExists = _context.Teams.Any(item =>
            item.TournamentId == team.TournamentId && item.TeamName == team.TeamName);

            if (teamNameExists)
            {
                return Conflict($"A team with the name '{team.TeamName}' already exists in this tournament.");
            }

            int regTeam = _context.Teams.Where((item) => item.TournamentId == team.TournamentId).Count();

            if (maxTeams - regTeam > 0)
            {
                _context.Teams.Add(team);
                await _context.SaveChangesAsync();
                return CreatedAtAction("GetTeam", new { id = team.TeamId }, team);
            }

            return Conflict("The maximum number of teams has already been reached for this tournament.");
        }


        // DELETE: api/Teams/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTeam(int id)
        {
            if (_context.Teams == null)
            {
                return NotFound();
            }
            var team = await _context.Teams.FindAsync(id);
            if (team == null)
            {
                return NotFound();
            }

            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TeamExists(int id)
        {
            return (_context.Teams?.Any(e => e.TeamId == id)).GetValueOrDefault();
        }
    }
}
