using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Polybeat
{
    public class Layer
    {
        private List<IDrawable> objectsOnLayer;

        private static List<IUpdatable> updatedObjects;

        public static Layer[] Layers;

        const int LAYER_COUNT = 10;

        /// <summary>
        /// Are objects on this layer drawn.
        /// </summary>
        public bool IsVisible { get; set; }

        static Layer()
        {
            Layers = new Layer[LAYER_COUNT];

            for (int i = 0; i < LAYER_COUNT; i++)
            {
                Layers[i] = new Layer();
            }
        }

        public Layer()
        {
            objectsOnLayer = new List<IDrawable>();
            updatedObjects = new List<IUpdatable>();
            IsVisible = true;
        }

        /// <summary>
        /// Lisää olion kerrokselle.
        /// </summary>
        /// <param name="obj">Lisättävä olio</param>
        /// <param name="layer">Kerros, johon olio lisätään. -5...0...5.</param>
        public static void AddToDraw(IDrawable obj, int layer)
        {
            Layers[layer + LAYER_COUNT / 2].objectsOnLayer.Add(obj);
        }

        /// <summary>
        /// Lisää olion kerrokselle.
        /// </summary>
        /// <param name="obj">Lisättävä olio</param>
        public static void AddToDraw(IDrawable obj)
        {
            AddToDraw(obj, 0);
        }

        /// <summary>
        /// Poistaa olion tietyltä kerrokselta.
        /// </summary>
        /// <param name="obj">Poistettava olio.</param>
        /// <param name="layer">Kerros, johon olio lisätään. -5...0...5.</param>
        public static bool RemoveFromDraw(IDrawable obj, int layer)
        {
            return Layers[layer + LAYER_COUNT / 2].objectsOnLayer.Remove(obj);
        }

        /// <summary>
        /// Poistaa olion, kun kerrosta ei tunneta.
        /// Käy läpi kaikki kerrokset, kunnes olio löytyy.
        /// </summary>
        /// <param name="obj">Poistettava olio.</param>
        /// <returns></returns>
        public static bool RemoveFromDraw(IDrawable obj)
        {
            for (int i = 0; i < Layers.Length; i++)
            {
                if (Layers[i].objectsOnLayer.Remove(obj))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Merkitsee olion päivitettäväksi.
        /// </summary>
        /// <param name="obj"></param>
        public static void AddToUpdate(IUpdatable obj)
        {
            updatedObjects.Add(obj);
        }

        /// <summary>
        /// Poistaa olion päivitettävien joukosta.
        /// </summary>
        /// <param name="obj"></param>
        public static void RemoveFromUpdate(IUpdatable obj)
        {
            updatedObjects.Remove(obj);
        }

        /// <summary>
        /// Lopettaa olion piirtämisen ja päivittämisen.
        /// </summary>
        /// <param name="obj"></param>
        public static void Remove(object obj)
        {
            IDrawable iD = obj as IDrawable;

            if (iD != null)
                RemoveFromDraw(iD);

            IUpdatable iU = obj as IUpdatable;

            if (iU != null)
                RemoveFromUpdate(iU);
        }

        /// <summary>
        /// Tyhjentää kaikki kerrokset.
        /// </summary>
        public static void ClearAllLayers()
        {
            for (int i = 0; i < Layers.Length; i++)
            {
                Layers[i].Clear();
            }

            updatedObjects.Clear();
        }

        /// <summary>
        /// Tyhjentää kerroksen.
        /// </summary>
        public void Clear()
        {
            this.objectsOnLayer.Clear();
        }

        /// <summary>
        /// Piirtää oliot tällä kerroksella.
        /// </summary>
        /// <param name="batch"></param>
        public void Draw(SpriteBatch batch)
        {
            if (!IsVisible) return;

            for (int i = 0; i < objectsOnLayer.Count; i++)
            {
                objectsOnLayer[i].Draw(batch);
            }
        }

        public static void Update(GameTime time)
        {
            for (int i = 0; i < updatedObjects.Count; i++)
            {
                updatedObjects[i].Update(time);
            }
        }
    }
}