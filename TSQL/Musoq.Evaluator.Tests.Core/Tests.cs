using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Musoq.Evaluator.Tests.Core
{
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



    [TestClass]
    public class Tests : TestBase
    {
        [TestMethod]
        public void Test3()

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
}
