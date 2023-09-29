using System;
using System.Collections.Generic;
using System.Text;

using Arch.Core;
using Arch.Core.Extensions;

namespace Arch.Archetype.Issue1
{
    internal class Program
    {
        private const int ENTITIES_COUNT = 1000;
        private const int GAME_LOOP_ITERATIONS_COUNT = 1000;

        public static void Main()
        {
            var world = World.Create();
            var random = new Random();

            #region Initialize the world
            for (int i = 0; i < ENTITIES_COUNT; i++)
            {
                world.Create(
                    new PermanentComponent(),
                    new TempComponent1()
                    {
                        //I'm using Random here to imitate complex game logic, when different entities has different archetype changing in different frames
                        RemainedIterations = random.Next(1, 500)
                    }
                );
            }
            #endregion

            #region "Game loop"
            for (int i = 0; i < GAME_LOOP_ITERATIONS_COUNT; i++)
            {
                var tempComponent1Query = new QueryDescription()
                    .WithAll<PermanentComponent, TempComponent1>();

                world.Query(
                    in tempComponent1Query,
                    (in Entity entity, ref TempComponent1 component) =>
                    {
                        if (--component.RemainedIterations == 0)
                        {
                            entity.Add(
                                new TempComponent2()
                                {
                                    //I'm using Random here to imitate complex game logic, when different entities has different archetype changing in different frames
                                    RemainedIterations = random.Next(1, 500)
                                }
                            );
                            entity.Remove<TempComponent1>();
                        }
                    }
                );


                var tempComponent2Query = new QueryDescription()
                    .WithAll<PermanentComponent, TempComponent2>();

                world.Query(
                    in tempComponent1Query,
                    (in Entity entity, ref TempComponent1 component) =>
                    {
                        if (--component.RemainedIterations == 0)
                        {
                            entity.Remove<TempComponent1>();
                        }
                    }
                );

                //Now I expecting only one archetype with only PermanentComponent...
            }
            #endregion

            #region Collect resulting entity archetypes
            var outputMessagesSet = new HashSet<string>();

            var allEntitiesQuery = new QueryDescription()
                .WithAll<PermanentComponent>();

            world.Query(
                in allEntitiesQuery,
                (in Entity entity) =>
                {
                    var sb = new StringBuilder();
                    var archetype = entity.GetArchetype();

                    sb.AppendLine($"Entities count: {archetype.Entities.ToString()}");
                    sb.AppendLine(String.Join(", ", archetype.Types));

                    outputMessagesSet.Add(sb.ToString());
                }
            );


            foreach (string msg in outputMessagesSet)
            {
                //...but I have several archetypes with TempComponent1 and TempComponent2
                Console.WriteLine(msg);
            }
            #endregion
        }
    }

    internal struct PermanentComponent
    {
        public int SomeData;
    }

    internal struct TempComponent1
    {
        public int SomeData;
        public int RemainedIterations;
    }

    internal struct TempComponent2
    {
        public int SomeData;
        public int RemainedIterations;
    }
}