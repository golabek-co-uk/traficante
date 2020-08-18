using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Traficante.TSQL.Evaluator.Tests.Core
{
    [TestClass]
    public class QueryableTests : TestBase
    {
        [TestMethod]
        public void GroupByTest()
        {
            var trainers = new List<Trainer>();
            trainers.Add(new Trainer { Id = 1, Name = "A:Jon" });
            trainers.Add(new Trainer { Id = 2, Name = "A:Mark" });
            trainers.Add(new Trainer { Id = 3, Name = "B:Adam" });
            var groupedResults = Queryable.GroupBy(trainers.AsQueryable(), x => new { a = x.Name.Substring(0, 1) });
            var result = Queryable.Select(groupedResults, x => x.Key.a);
            Assert.IsTrue(result.Count() == 2);
        }

        [TestMethod]
        public void JoinTest()
        {
            var trainers = new List<Trainer>();
            trainers.Add(new Trainer { Id = 1, Name = "Jon" });
            trainers.Add(new Trainer { Id = 2, Name = "Mark" });

            var pets = new List<Pet>();
            pets.Add(new Pet { Id = 1, Name = "Bigos" });
            pets.Add(new Pet { Id = 2, Name = "Bacon" });
            pets.Add(new Pet { Id = 3, Name = "Banger" });

            var trainersPets = new List<TrainerPet>();
            trainersPets.Add(new TrainerPet { TrainerId = 1, PetId = 1 });
            trainersPets.Add(new TrainerPet { TrainerId = 1, PetId = 2 });
            trainersPets.Add(new TrainerPet { TrainerId = 2, PetId = 3 });
   
            var x = Queryable.SelectMany<Trainer,Output>(trainers.AsQueryable(),
                trainer =>
                    Queryable.Select<TrainerPet,Output>(
                        Queryable.Where<TrainerPet>(trainersPets.AsQueryable(), trainerPet => trainerPet.TrainerId == trainer.Id),
                        xx => new Output { Trainer = trainer , TrainerPet = xx } )
                );

            var join = trainers.SelectMany(trainer =>
            {
                return trainersPets.Where(trainerPet => trainerPet.TrainerId == trainer.Id)
                                   .SelectMany(trainerPet =>
                                   {
                                       return pets.Where(pet => pet.Id == trainerPet.PetId)
                                                          .Select(pet =>
                                                              new Output
                                                              {
                                                                  Trainer = trainer,
                                                                  TrainerPet = trainerPet,
                                                                  Pet = pet
                                                              });
                                   });
            }).ToList();
        }
    }

    class Trainer
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class TrainerPet
    {
        public int TrainerId { get; set; }
        public int PetId { get; set; }
    }

    class Pet
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    class Output
    {
        public Trainer Trainer { get; set; }
        public TrainerPet TrainerPet { get; set; }
        public Pet Pet { get; set; }
    }
}
