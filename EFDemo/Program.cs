using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EFDemo
{
    class Program
    {
        static void Main()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

            var options = new DbContextOptionsBuilder<PagilaContext>()
                .UseNpgsql(config.GetConnectionString("Pagila"))
                .Options;

            using var context = new PagilaContext(options);

            GetActors(context);

            int actorId = InsertActor(context);

            GetActors(context, "Collins");

            UpdateActor(context, actorId);

            GetActors(context, "NewLastName");

            DeleteActor(context, actorId);
        }

        static void GetActors(PagilaContext context)
        {
            Console.WriteLine("First 5 Actors:");

            var actors = context.Actors
                .OrderBy(a => a.ActorId)
                .Take(5)
                .ToList();

            foreach (var actor in actors)
            {
                Console.WriteLine($"{actor.ActorId}: {actor.FirstName} {actor.LastName}");
            }

            Console.WriteLine();
        }

        static void GetActors(PagilaContext context, string lastName)
        {
            Console.WriteLine($"Actors with last name of {lastName}:");

            var actors = context.Actors
                .Where(a => a.LastName == lastName)
                .OrderBy(a => a.ActorId)
                .ToList();

            foreach (var actor in actors)
            {
                Console.WriteLine($"{actor.ActorId}: {actor.FirstName} {actor.LastName}");
            }

            Console.WriteLine();
        }

        static int InsertActor(PagilaContext context)
        {
            var actor = new Actor
            {
                FirstName = "Kimberly",
                LastName = "Collins",
                LastUpdate = DateTime.UtcNow
            };

            context.Actors.Add(actor);
            context.SaveChanges();

            Console.WriteLine($"Inserted actor with ID {actor.ActorId}");
            Console.WriteLine();

            return actor.ActorId;
        }

        static void GetFilms(PagilaContext context, int releaseYear)
        {
            Console.WriteLine($"Films released in {releaseYear}:");

            var films = context.Films
                .Include(a => a.Actors)
                .Where(a => a.ReleaseYear == releaseYear)
                .OrderBy(a => a.FilmId)
                .ToList();

            foreach (var film in films)
            {
                Console.WriteLine($"{film.FilmId}: {film.Title} ({film.Actors.Count} actors)");
            }

            Console.WriteLine();
        }

        static int InsertFilm(PagilaContext context, string title)
        {
            var film = new Film
            {
                Title = title,
                Description = null,
                ReleaseYear = 2026
            };

            context.Films.Add(film);
            context.SaveChanges();

            return film.FilmId;
        }

        private static void InsertFilmActor(PagilaContext context, int actorId, int filmId)
        {
            var actor = context.Actors
                .FirstOrDefault(a => a.ActorId == actorId);

            var film = context.Films
                .Include(f => f.Actors)
                .FirstOrDefault(f => f.FilmId == filmId);

            if (film != null && actor != null)
            {
                if (!film.Actors.Any(a => a.ActorId == actor.ActorId))
                {
                    film.Actors.Add(actor);
                    context.SaveChanges();
                    Console.WriteLine($"Added {actor.FirstName} {actor.LastName} to {film.Title}");
                }
                else
                {
                    Console.WriteLine($"Actor {actor.FirstName} {actor.LastName} is already in the film {film.Title}.");
                }
            }
        }

        static void UpdateActor(PagilaContext context, int actorId)
        {
            var actor = context.Actors.Find(actorId);

            if (actor != null)
            {
                actor.LastName = "NewLastName";
                actor.LastUpdate = DateTime.UtcNow;

                context.SaveChanges();
                Console.WriteLine($"Updated actor {actorId}");

                Console.WriteLine();
            }
        }

        static void DeleteActor(PagilaContext context, int actorId)
        {
            var actor = context.Actors.Find(actorId);

            if (actor != null)
            {
                context.Actors.Remove(actor);
                context.SaveChanges();
                Console.WriteLine($"Deleted actor {actorId}");
            }
        }
    }
}
