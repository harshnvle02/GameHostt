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
    public class UsersController : ControllerBase
    {
        private readonly GameHostDbContext _context;

        public UsersController(GameHostDbContext context)
        {
            _context = context;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
          if (_context.Users == null)
          {
              return NotFound();
          }
            return await _context.Users.ToListAsync();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
          if (_context.Users == null)
          {
              return NotFound();
          }
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User user)
        {
            if (id != user.UserId)
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
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

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            if (_context.Users == null)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, "Database context is unavailable.");
            }

            // Check if the username already exists
            bool userExistsName = await _context.Users.AnyAsync(u => u.Username == user.Username);

            if (userExistsName)
            {
                return StatusCode(StatusCodes.Status409Conflict, new { message = "Username already exists." });
            }

            bool userExistsEmail = await _context.Users.AnyAsync(u => u.Email == user.Email);

            if (userExistsEmail)
            {
                return StatusCode(StatusCodes.Status409Conflict, new { message = "Email address already exists." });
            }

            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return StatusCode(StatusCodes.Status201Created, user);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }


        [HttpPost("login")]
        public async Task<ActionResult<User>> ReturnUser(User user)
        {
            if (_context.Users == null)
            {
                return Problem("Entity set 'GameHostDbContext.Users'  is null.");
            }

            var response = _context.Users.FirstOrDefault((item) => item.Username.Equals(user.Username) && item.Email.Equals(user.Email) && item.PasswordHash.Equals(user.PasswordHash));
            if(response==null)
            {
                return Unauthorized("Invalid Credential");
            }

            return Ok(response);
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (_context.Users == null)
            {
                return NotFound();
            }
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return (_context.Users?.Any(e => e.UserId == id)).GetValueOrDefault();
        }
    }
}
