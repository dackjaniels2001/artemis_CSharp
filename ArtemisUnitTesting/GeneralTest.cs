using System;
using Artemis;
using ArtemisTest.Components;
using ArtemisTest.System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArtemisTest
{
    [TestClass]
    public class GeneralTest
	{
		static Bag<Component> healthBag = new Bag<Component>();
		static Dictionary<Type,Bag<Component>> componentPool = new Dictionary<Type, Bag<Component>>();			
			
		private void RemovedComponent(Entity e,Component c) 
      	{
        	 Console.WriteLine("This was the component removed: "+(c.GetType()));
			 Bag<Component> tempBag;
			 componentPool.TryGetValue(c.GetType(),out tempBag);
			 Console.WriteLine("Health Component Pool has "+tempBag.Size+" objects");
			 tempBag.Add(c);
			 componentPool.TryGetValue(c.GetType(),out tempBag);
			 Console.WriteLine("Health Component Pool now has "+tempBag.Size+" objects");
      	}
		
		private void RemovedEntity(Entity e) 
      	{
        	 Console.WriteLine("This was the entity removed: "+(e.UniqueId));
      	}


        [TestMethod]
        public void multi()
        {
            healthBag.Add(new Health());
            healthBag.Add(new Health());
            componentPool.Add(typeof(Health), healthBag);
                        
            EntityWorld world = new EntityWorld();
            SystemManager systemManager = world.SystemManager;
            world.EntityManager.RemovedComponentEvent += new RemovedComponentHandler(RemovedComponent);
            world.EntityManager.RemovedEntityEvent += new RemovedEntityHandler(RemovedEntity);

            EntitySystem hs = systemManager.SetSystem(new MultHealthBarRenderSystem(), ExecutionType.UpdateSynchronous);            
            world.InitializeAll(false);

            List<Entity> l = new List<Entity>();
            for (int i = 0; i < 1000; i++)
            {
                Entity et = world.CreateEntity();
                et.AddComponent(new Health());
                et.GetComponent<Health>().HP += 100;
                et.Refresh();
                l.Add(et);
            }

            for (int i = 0; i < 100; i++)
            {
                DateTime dt = DateTime.Now;
                world.Update(0);
                Console.WriteLine((DateTime.Now - dt).TotalMilliseconds);
            }            

            int df = 0;
            foreach (var item in l)
            {
                if (item.GetComponent<Health>().HP == 90)
                {
                    df++;
                }
            }

             
        }
        [TestMethod]
        public void multsystem()
        {
            healthBag.Clear();
            componentPool.Clear();

            healthBag.Add(new Health());
            healthBag.Add(new Health());
            componentPool.Add(typeof(Health), healthBag);
            
            EntityWorld world = new EntityWorld();
            SystemManager systemManager = world.SystemManager;
            world.EntityManager.RemovedComponentEvent += new RemovedComponentHandler(RemovedComponent);
            world.EntityManager.RemovedEntityEvent += new RemovedEntityHandler(RemovedEntity);            
            EntitySystem hs = systemManager.SetSystem(new SingleHealthBarRenderSystem(),ExecutionType.UpdateAsynchronous);
            hs = systemManager.SetSystem(new DummySystem(),ExecutionType.UpdateAsynchronous);
            hs = systemManager.SetSystem(new DummySystem2(), ExecutionType.UpdateAsynchronous);
            hs = systemManager.SetSystem(new DummySystem3(), ExecutionType.UpdateAsynchronous);
            world.InitializeAll(false);
            

            List<Entity> l = new List<Entity>();
            for (int i = 0; i < 100000; i++)
            {
                Entity et = world.CreateEntity();
                et.AddComponent(new Health());
                et.GetComponent<Health>().HP += 100;
                et.Refresh();
                l.Add(et);
            }

            for (int i = 0; i < 100; i++)
            {
                DateTime dt = DateTime.Now;
                world.Update(0,ExecutionType.UpdateAsynchronous);
                //systemManager.UpdateSynchronous(ExecutionType.Update);
                Console.WriteLine((DateTime.Now - dt).TotalMilliseconds);
            }

            //int df = 0;
            //foreach (var item in l)
            //{
            //    if (item.GetComponent<Health>().GetHealth() == 90)
            //    {
            //        df++;
            //    }
            //    else
            //    {
            //        Console.WriteLine("errro");
            //    }
            //}            
        }

        [TestMethod]
        public void SecondMostSimpleSystemEverTest()
        {
            EntityWorld world = new EntityWorld();            
            world.InitializeAll(true);           


            Entity et = world.CreateEntity();
            et.AddComponent(new Health());
            et.GetComponent<Health>().HP = 100;
            et.Refresh();
            
            Entity et1 = world.CreateEntity();        
            et1.AddComponent(new Power());       
            et1.GetComponent<Power>().POWER = 100;
            et1.Refresh();

            {                
                world.Update(0);                
            }

            ///two systems runnning
            ///each remove 10 HP
            Debug.Assert(et.GetComponent<Health>().HP == 80);           
            Debug.Assert(et1.GetComponent<Power>().POWER == 90);

        }


        [TestMethod]
        public void MostSimpleSystemEverTest()
        {
            EntityWorld world = new EntityWorld();
            SystemManager systemManager = world.SystemManager;
            MostSimpleSystemEver DummyCommunicationSystem = new MostSimpleSystemEver();
            systemManager.SetSystem(DummyCommunicationSystem, ExecutionType.UpdateSynchronous);
            world.InitializeAll(false);
                        
            
                Entity et = world.CreateEntity();
                et.AddComponent(new Health());
                et.GetComponent<Health>().HP = 100;                
                et.Refresh();           

            
                Entity et1 = world.CreateEntity();
                et1.AddComponent(new Health());
                et1.AddComponent(new Power());
                et1.GetComponent<Health>().HP = 100;
                et1.GetComponent<Power>().POWER = 100;
                et1.Refresh();
            

            {
                DateTime dt = DateTime.Now;
                world.Update(0);
                Console.WriteLine((DateTime.Now - dt).TotalMilliseconds);
            }

            Debug.Assert(et.GetComponent<Health>().HP == 90);
            Debug.Assert(et1.GetComponent<Health>().HP == 100);
            Debug.Assert(et1.GetComponent<Power>().POWER == 100);

        }

        [TestMethod]
        public void DummyTests()
        {
            EntityWorld world = new EntityWorld();
            SystemManager systemManager = world.SystemManager;
            DummyCommunicationSystem DummyCommunicationSystem = new DummyCommunicationSystem();
            systemManager.SetSystem(DummyCommunicationSystem, ExecutionType.UpdateSynchronous);
            world.InitializeAll(false);

            for (int i = 0; i < 100; i++)
            {
                Entity et = world.CreateEntity();
                et.AddComponent(new Health());
                et.GetComponent<Health>().HP += 100;
                et.Group = "teste";
                et.Refresh();
            }

            {
                Entity et = world.CreateEntity();
                et.Tag = "tag";
                et.AddComponent(new Health());
                et.GetComponent<Health>().HP += 100;             
                et.Refresh();
            }

            {
                DateTime dt = DateTime.Now;
                world.Update(0);                
                Console.WriteLine((DateTime.Now - dt).TotalMilliseconds);
            }

            Debug.Assert(world.TagManager.GetEntity("tag") != null);
            Debug.Assert(world.GroupManager.GetEntities("teste").Size == 100);
            Debug.Assert(world.EntityManager.ActiveEntitiesCount == 101);
            Debug.Assert(world.SystemManager.Systems.Size == 1);



        }
        [TestMethod]
        public  void SystemComunicationTeste()
        {
            EntitySystem.BlackBoard.SetEntry<int>("Damage", 5);

            EntityWorld world = new EntityWorld();
            SystemManager systemManager = world.SystemManager;
            DummyCommunicationSystem DummyCommunicationSystem = new DummyCommunicationSystem();
            systemManager.SetSystem(DummyCommunicationSystem, ExecutionType.UpdateSynchronous);
            world.InitializeAll(false);            

            List<Entity> l = new List<Entity>();
            for (int i = 0; i < 100; i++)
            {
                Entity et = world.CreateEntity();
                et.AddComponent(new Health());
                et.GetComponent<Health>().HP += 100;
                et.Refresh();
                l.Add(et);
            }
            
            {
                DateTime dt = DateTime.Now;
                world.Update(0);
                Console.WriteLine((DateTime.Now - dt).TotalMilliseconds);
            }

            EntitySystem.BlackBoard.SetEntry<int>("Damage", 10);

            {
                DateTime dt = DateTime.Now;
                world.Update(0);
                Console.WriteLine((DateTime.Now - dt).TotalMilliseconds);
            }

            foreach (var item in l)
            {
                Debug.Assert(item.GetComponent<Health>().HP == 85);
            }
            
        }

        [TestMethod]
        public void AttributesTestsMethod()
        {
            EntityWorld world = new EntityWorld();
            world.PoolCleanupDelay = 1;
            world.InitializeAll();

            Debug.Assert(world.SystemManager.Systems.Size == 2);

            Entity et = world.CreateEntity();
            var power = et.AddComponentFromPool<Power2>();
            power.POWER = 100;
            et.Refresh();

            Entity et1 = world.CreateEntityFromTemplate("teste");
            Debug.Assert(et1 != null);

            {
                world.Update(0, ExecutionType.UpdateSynchronous);
            }

            et.RemoveComponent<Power2>();
            et.Refresh();

            {
                world.Update(0, ExecutionType.UpdateSynchronous);
            }

            et.AddComponentFromPool<Power2>();
            et.GetComponent<Power2>().POWER = 100;
            et.Refresh();

            world.Update(0, ExecutionType.UpdateSynchronous);
        }

        [TestMethod]
        public void QueueSystemTeste()
        {
            EntityWorld world = new EntityWorld();
            SystemManager systemManager = world.SystemManager;
            QueueSystemTest QueueSystemTest = new ArtemisTest.QueueSystemTest();
            QueueSystemTest QueueSystemTest2 = new ArtemisTest.QueueSystemTest();
            systemManager.SetSystem(QueueSystemTest, ExecutionType.UpdateAsynchronous);
            systemManager.SetSystem(QueueSystemTest2, ExecutionType.UpdateAsynchronous);

            QueueSystemTest2 QueueSystemTestteste = new ArtemisTest.QueueSystemTest2();
            systemManager.SetSystem(QueueSystemTestteste, ExecutionType.UpdateAsynchronous);

            world.InitializeAll(false);

            QueueSystemTest.SetQueueProcessingLimit(20, QueueSystemTest.Id);
            Debug.Assert(QueueSystemTest.GetQueueProcessingLimit(QueueSystemTest.Id) == QueueSystemTest.GetQueueProcessingLimit(QueueSystemTest2.Id));


            
            Debug.Assert(QueueSystemTest.GetQueueProcessingLimit(QueueSystemTestteste.Id) != QueueSystemTest.GetQueueProcessingLimit(QueueSystemTest2.Id));

            QueueSystemTest.SetQueueProcessingLimit(1000, QueueSystemTest.Id);
            QueueSystemTest.SetQueueProcessingLimit(2000, QueueSystemTestteste.Id);

            List<Entity> l = new List<Entity>();
            for (int i = 0; i < 1000000; i++)
            {
                Entity et = world.CreateEntity();
                et.AddComponent(new Health());
                et.GetComponent<Health>().HP = 100;
                QueueSystemTest.AddToQueue(et, QueueSystemTest.Id);
                l.Add(et);
            }
            
            List<Entity> l2 = new List<Entity>();
            for (int i = 0; i < 1000000; i++)
            {
                Entity et = world.CreateEntity();
                et.AddComponent(new Health());
                et.GetComponent<Health>().HP = 100;
                QueueSystemTest.AddToQueue(et, QueueSystemTestteste.Id);
                l2.Add(et);
            }

            Console.WriteLine("Start");
            while (QueueSystemTest.QueueCount(QueueSystemTest.Id) > 0 || QueueSystemTest.QueueCount(QueueSystemTestteste.Id) > 0)
            {
                DateTime dt = DateTime.Now;
                world.Update(0, ExecutionType.UpdateAsynchronous);                
                Console.WriteLine("Count: " + QueueSystemTest.QueueCount(QueueSystemTest.Id));
                Console.WriteLine("Time: " + (DateTime.Now - dt).TotalMilliseconds);

            }
            Console.WriteLine("End");

            foreach (var item in l)
            {
                Debug.Assert(item.GetComponent<Health>().HP == 90);
            }
            foreach (var item in l2)
            {
                Debug.Assert(item.GetComponent<Health>().HP == 80);
            }
        }

        [TestMethod]
        public void HybridQueueSystemTeste()
        {

            EntityWorld world = new EntityWorld();
            SystemManager systemManager = world.SystemManager;
            HybridQueueSystemTest HybridQueueSystemTest = new ArtemisTest.HybridQueueSystemTest();
            EntitySystem hs = systemManager.SetSystem(HybridQueueSystemTest, ExecutionType.UpdateSynchronous);
            world.InitializeAll(false);

            List<Entity> l = new List<Entity>();
            for (int i = 0; i < 100; i++)
            {
                Entity et = world.CreateEntity();
                et.AddComponent(new Health());
                et.GetComponent<Health>().HP += 100;
                et.Refresh();
                l.Add(et);
            }

            for (int i = 0; i < 100; i++)
            {
                Entity et = world.CreateEntity();
                et.AddComponent(new Health());
                et.GetComponent<Health>().HP += 100;
                HybridQueueSystemTest.
                    AddToQueue(et);
                l.Add(et);
            }

            int j = 0;
            while (HybridQueueSystemTest.QueueCount > 0) 
            {
                j++;
                DateTime dt = DateTime.Now;
                world.Update(0);
                Console.WriteLine((DateTime.Now - dt).TotalMilliseconds);
            }

            for (int i = 0; i < 100; i++)
            {
                Debug.Assert(l[i].GetComponent<Health>().HP == 100 - (10 * j));
            }

            for (int i = 100; i < 200; i++)
            {
                Debug.Assert(l[i].GetComponent<Health>().HP == 90);
            }
            
        }
          
	}
}