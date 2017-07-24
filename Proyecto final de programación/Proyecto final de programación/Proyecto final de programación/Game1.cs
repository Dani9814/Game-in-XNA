using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace Proyecto_final_de_programación
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        
        #region Atributos generales
        GraphicsDeviceManager graphics;
        Model boxModel;
        Model sueloModel;
        Model movilModel;
        Model characterModel;
        Vector3 posicionPj;
        int boxmesure;
        float aspectRatio;
        Vector3 cameraPosition;
        Vector3 cameraCentrado;
        float boxRotation;
        float tiempo;
        int [,] map;
        int mapWidth =16;
        int mapHeight=16;
        float speed;
        Vector3 desplaza;
        personaje Jugador;
        CajasMoviles caja;
        Vector3 posicionPjreal;
        Vector3 posicionPjrealfutura;
        SpriteFont spFont;
        SpriteBatch spBatch;
        bool ganar;
        #endregion
        #region Métodos de inicializacion y carga
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            this.Window.AllowUserResizing= true;
        }

        protected override void Initialize()
        {
            posicionPj = new Vector3(1, 1, 0);
            boxmesure = 100;
            aspectRatio = graphics.GraphicsDevice.Viewport.AspectRatio;
            boxRotation = 0;
            speed = 25;
            caja = new CajasMoviles(speed, boxmesure);
            desplaza = Vector3.Zero;
            posicionPjreal = Vector3.Zero;
            posicionPjrealfutura = Vector3.Zero;
            Jugador = new personaje(new Vector3(1,1,0),1,speed,boxmesure);
            map = new int[,]{
            {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
            {1,0,0,1,1,0,0,0,1,1,1,0,0,0,1,1},
            {1,0,0,1,1,0,0,0,0,2,0,0,0,0,0,1},
            {1,0,0,1,1,0,1,1,1,1,1,1,1,0,0,1},
            {1,0,0,1,1,0,1,1,1,1,1,1,1,0,1,1},
            {1,0,0,0,0,0,1,1,1,1,1,1,1,0,1,1},
            {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,0,0,1,1,0,0,0,0,0,0,0,2,0,0,1},
            {1,0,0,1,1,2,1,1,1,1,1,1,1,0,0,1},
            {1,0,0,1,1,0,1,1,1,1,0,0,0,0,0,1},
            {1,0,0,1,0,0,0,0,0,0,0,0,0,0,0,1},
            {1,0,0,1,0,0,2,0,1,0,0,1,1,1,1,1},
            {1,0,0,1,0,0,1,0,1,0,0,0,0,0,0,1},
            {1,0,0,1,1,0,1,0,0,0,0,1,1,1,1,1},
            {1,0,0,1,1,0,1,0,0,0,1,1,1,1,1,1},
            {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
            };

            ganar = false;            
            base.Initialize();
        }

        
        protected override void LoadContent()
        {
            boxModel = Content.Load<Model>("Models\\crate");
            sueloModel = Content.Load<Model>("Models\\crate1");
            characterModel = Content.Load<Model>("Models\\android");
            movilModel = Content.Load<Model>("Models\\crate3");
            spFont = Content.Load<SpriteFont>(@"Arial");
            spBatch = new SpriteBatch(graphics.GraphicsDevice);
        }

       
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }
        #endregion

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            
            ganarjuego();
            tiempo = (float)gameTime.TotalGameTime.TotalSeconds;

            Jugador.MovimientoPersonaje(tiempo,map);
            if (Jugador.choque)
                caja.Movimientocajas(ref Jugador.direcciondechoque, ref Jugador.puntodechoque, ref map, ref Jugador.choque);
            
            if (Keyboard.GetState().IsKeyDown(Keys.V))
            {
                cameraPosition = Escena(8, 8, 0);
                cameraCentrado = Escena(8, 8, 0);
                cameraPosition.Z += 2000.0f;
            }
            else
            {
                cameraPosition = Jugador.posicionreal;
                cameraCentrado = Jugador.posicionreal;
                cameraPosition.Z += 700.0f;
            }
            
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            if (Jugador.choque)
                DibujarModelo(movilModel, caja.posicionreal);
            DibujarModelo(characterModel, Jugador.posicionreal);
            DibujarMapa();

            if (ganar)
            {
                spBatch.Begin();
                spBatch.DrawString(spFont, "Ganaste,presiona R para empezar de nuevo", new Vector2(200f, 250f), Color.Red);
                spBatch.End();
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            }
           
            base.Draw(gameTime);
        }
        #region Métodos de dibujo
        public void DibujarModelo(Model model, Vector3 position)
        {

            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();

                    effect.World = transforms[mesh.ParentBone.Index] * Matrix.CreateTranslation(position) * Matrix.CreateRotationY(boxRotation);
                    effect.View = Matrix.CreateLookAt(cameraPosition, cameraCentrado, Vector3.Up);
                    effect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), aspectRatio, 1.0f, 10000.0f);
                }
                mesh.Draw();
            }
        }

        public void DibujarMapa ()
        {
             for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    if (map[x, y] == 1 )
                    {
                        DibujarModelo(boxModel, Escena(x, y, 0));
                       
                    }
                    else 
                        
                     if(map[x,y]==2)
                         DibujarModelo(movilModel,Escena(x,y,0));
                     else
                         if((x==12)&&(y==14||y==13||y==12||y==11))
                             DibujarModelo(movilModel, Escena(x, y,-1));
                         else
                            DibujarModelo(sueloModel, Escena(x, y, -1));
                }
            }
        }
        void ganarjuego()
        {
            if (map[12, 11] == 2 && map[12, 12] == 2 && map[12, 13] == 2 && map[12, 14] == 2)
            {
                ganar = true;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.R))
            {
                Initialize();
            }
        }

        public Vector3 Escena(float x, float y, float z)
        {
            return new Vector3(x * boxmesure, y * boxmesure, z * boxmesure);
        }
        #endregion
     
    }
    public class personaje
    {
        #region Parámetros
        public Vector3 posicion;
        public Vector3 posicionfutura;
        public Vector3 posicionreal;
        public Vector3 posicionrealfutura;
        public Vector3 puntodechoque;
        public Vector3 direcciondechoque;
        Vector3 desplaza;
        int ide;
        float distanciaincial = 0;
        float distanciaactual = 0;
        float boxmesure;
        public float speed;
         bool trans=false;
        public bool choque = false;
         int actualizado;
         float startime;
         float currenttime;
        
        #endregion
        #region Constructor de clase
        public personaje(Vector3 posicion, int ide,float speed,int boxmesure)
        {
            this.ide = ide;
            this.posicion = posicion;
            this.speed = speed;
            this.boxmesure = boxmesure;
            posicionfutura = posicion;
            posicionreal = Escena(posicion.X, posicion.Y, posicion.Z);
            posicionrealfutura = Escena(posicionfutura.X, posicionfutura.Y, posicionfutura.Z);
            desplaza = Vector3.Zero;
            puntodechoque=Vector3.Zero;
            direcciondechoque=Vector3.Zero;
               
        }
        public personaje()
        {
        }
        #endregion
        #region Funciones auxiliares de Movimiento Personaje.
        bool  EsPosibleIr(int[,] mapa,int x , int y)
        {
            bool posible=false;

            if (mapa[(int)posicionfutura.X + x, (int)posicionfutura.Y + y] == 0 || mapa[(int)posicionfutura.X + x, (int)posicionfutura.Y + y] == 4)
                posible = true;
            else if (mapa[(int)posicionfutura.X + x, (int)posicionfutura.Y + y] ==2 )
                choques(posicionfutura, x, y);

            return posible;
        }
        int botonpulsado()  //devuelve 0 si no hay tecla ,1 si es w ,2 si es A ,3 si es S, 4 si es D.
        {
            int i =0;

            if (Keyboard.GetState().IsKeyDown(Keys.W) && (!Keyboard.GetState().IsKeyDown(Keys.A))&&(!Keyboard.GetState().IsKeyDown(Keys.S))&&(!Keyboard.GetState().IsKeyDown(Keys.D)))
            {
                i = 1;
            }

            else if (!Keyboard.GetState().IsKeyDown(Keys.W) && (Keyboard.GetState().IsKeyDown(Keys.A)) && (!Keyboard.GetState().IsKeyDown(Keys.S)) && (!Keyboard.GetState().IsKeyDown(Keys.D)))
            {
                i = 2;
            }

            else if ((!Keyboard.GetState().IsKeyDown(Keys.W)) && (!Keyboard.GetState().IsKeyDown(Keys.A)) && (Keyboard.GetState().IsKeyDown(Keys.S)) && (!Keyboard.GetState().IsKeyDown(Keys.D)))
            {
                i = 3;
            }


            else if ((!Keyboard.GetState().IsKeyDown(Keys.W)) && (!Keyboard.GetState().IsKeyDown(Keys.A)) && (!Keyboard.GetState().IsKeyDown(Keys.S)) && (Keyboard.GetState().IsKeyDown(Keys.D)))
            {
                i = 4;
            }

            return i;
        }
        void actualizarposicion(int[,] mapa)
        {
            
            if (Keyboard.GetState().IsKeyDown(Keys.W) && (!Keyboard.GetState().IsKeyDown(Keys.A))&&(!Keyboard.GetState().IsKeyDown(Keys.S))&&(!Keyboard.GetState().IsKeyDown(Keys.D))&&EsPosibleIr(mapa,0,1))
            {
                posicionfutura.Y = posicionfutura.Y+1;
              
            }

            else if (!Keyboard.GetState().IsKeyDown(Keys.W) && (Keyboard.GetState().IsKeyDown(Keys.A)) && (!Keyboard.GetState().IsKeyDown(Keys.S)) && (!Keyboard.GetState().IsKeyDown(Keys.D))&&EsPosibleIr(mapa,-1,0))
            {
                posicionfutura.X = posicionfutura.X - 1;
                
            }

            else if ((!Keyboard.GetState().IsKeyDown(Keys.W)) && (!Keyboard.GetState().IsKeyDown(Keys.A)) && (Keyboard.GetState().IsKeyDown(Keys.S)) && (!Keyboard.GetState().IsKeyDown(Keys.D))&&EsPosibleIr(mapa,0,-1))
            {
                posicionfutura.Y = posicionfutura.Y - 1;
                
            }


            else if ((!Keyboard.GetState().IsKeyDown(Keys.W)) && (!Keyboard.GetState().IsKeyDown(Keys.A)) && (!Keyboard.GetState().IsKeyDown(Keys.S)) && (Keyboard.GetState().IsKeyDown(Keys.D))&& EsPosibleIr(mapa, 1, 0))
            {
                posicionfutura.X = posicionfutura.X + 1;
      
            }
        }
        #endregion
        #region Movimientopersonaje
        public void MovimientoPersonaje(float tiempo , int [,] mapa)
        {
            if (trans==false)
            {
                actualizado = botonpulsado();
                actualizarposicion(mapa);
                posicionreal = Escena(posicion.X, posicion.Y, posicion.Z);
                posicionrealfutura = Escena(posicionfutura.X, posicionfutura.Y, posicionfutura.Z);
                distanciaincial = (posicionrealfutura - posicionreal).Length();
                desplaza = (posicionrealfutura - posicionreal) / speed;
                trans = true;
                startime=tiempo; 
            }
            else
            {
                currenttime = tiempo;
                if (actualizado == botonpulsado()&&((currenttime-startime) >0.35))
                {
                    
                 actualizarposicion(mapa);
                 posicionrealfutura = Escena(posicionfutura.X, posicionfutura.Y, posicionfutura.Z);
                 distanciaincial = (posicionrealfutura - posicionreal).Length();
                 startime = currenttime;
                }
              
                distanciaactual = (posicionrealfutura - posicionreal).Length();
                if ((posicionrealfutura - posicionreal).Length() > 2)
                {
                    if (distanciaactual / distanciaincial > 0.25)
                        posicionreal += desplaza;
                    else if (distanciaactual / distanciaincial > 0.125)
                        posicionreal += desplaza * (float)0.75;
                    else if (distanciaactual / distanciaincial > 0.075)
                        posicionreal += desplaza * (float)0.5;
                    else if (distanciaactual / distanciaincial > 0)
                        posicionreal = posicionreal + desplaza * (float)0.35;
                }
                else
                {
                    posicion = posicionfutura;
                    posicionreal = posicionrealfutura;
                    trans = false;
                }
            }

     }
        #endregion
        #region Escena
        protected Vector3 Escena(float x, float y, float z)
        {
            return new Vector3(x * boxmesure, y * boxmesure, z * boxmesure);
        }
        #endregion 
        #region choques
        void choques(Vector3 punto,int x , int y)
        {
            puntodechoque = punto;
            direcciondechoque.X = x;
            direcciondechoque.Y = y;
            choque = true;
        }
        #endregion
    }
    public class CajasMoviles
    {
        #region Parámetros
        public Vector3 posicion;
        public Vector3 posicionfutura;
        public Vector3 posicionreal;
        public Vector3 posicionrealfutura;
        protected Vector3 desplaza;
        protected float distanciaincial = 0;
        protected float distanciaactual = 0;
        protected float boxmesure;
        public float speed;
        bool trans=false;
        public bool choquetrue = true;
        #endregion
        #region Constructor de clase
        public CajasMoviles(float speed,int boxmesure)
        {
            posicion = Vector3.Zero;
            posicionfutura=Vector3.Zero;
            this.speed = speed;
            this.boxmesure = boxmesure;
            posicionreal = Escena(posicion.X, posicion.Y, posicion.Z);
            posicionrealfutura = Escena(posicionfutura.X, posicionfutura.Y, posicionfutura.Z);
            desplaza = Vector3.Zero;
           
               
        }
        #endregion
        #region MovimientoCajas
        public void Movimientocajas(ref Vector3 direccion, ref Vector3 punto,ref int[,] mapa,ref bool choque)
        {
            if (trans == false)
            {
                
                actualizarcajas(direccion, punto, ref mapa);
                posicionreal = Escena(posicion.X, posicion.Y, posicion.Z);
                posicionrealfutura = Escena(posicionfutura.X, posicionfutura.Y, posicionfutura.Z);
                distanciaincial = (posicionrealfutura - posicionreal).Length();
                desplaza = (posicionrealfutura - posicionreal) / speed;
                distanciaactual = (posicionrealfutura - posicionreal).Length();
                trans = true;
            }
            else
            {
                if ((posicionrealfutura - posicionreal).Length() > 2)
                {
                    if (distanciaactual / distanciaincial > 0.25)
                        posicionreal += desplaza;
                    else if (distanciaactual / distanciaincial > 0.125)
                        posicionreal += desplaza * (float)0.75;
                    else if (distanciaactual / distanciaincial > 0.075)
                        posicionreal += desplaza * (float)0.5;
                    else if (distanciaactual / distanciaincial > 0)
                        posicionreal = posicionreal + desplaza * (float)0.35;
                }
                else
                {
                    mapa[(int)posicion.X, (int)posicion.Y] = 0;
                    mapa[(int)posicionfutura.X, (int)posicionfutura.Y] = 2;
                    posicion = posicionfutura;
                    posicionreal = posicionrealfutura;
                    direccion = Vector3.Zero;
                    punto = Vector3.Zero;
                    choque = false;
                    choquetrue = false;
                    trans = false;
                }
            }   


        }
        #endregion
        #region funcion axuliar de movimiento caja

        void actualizarcajas (Vector3 direccion ,Vector3 punto,ref int [,] mapa)
        {
            if (EsPosibleIr(mapa, direccion, punto))
            {
                posicion = direccion + punto;
                posicionfutura = posicion + direccion;
                mapa[(int)posicionfutura.X, (int)posicionfutura.Y] = 3;
                mapa[(int)posicion.X, (int)posicion.Y] = 3;
            }
            
        }
        public bool EsPosibleIr(int[,] mapa, Vector3 x, Vector3 y)
        {
            bool posible = false;

            if (mapa[(int)(2*x.X+ y.X), (int)(y.Y+2*x.Y)] == 0)
                posible = true;
            return posible;

        }

        #endregion
        #region Escena
        Vector3 Escena(float x, float y, float z)
        {
            return new Vector3(x * boxmesure, y * boxmesure, z * boxmesure);
        }
        #endregion 
        
        }
       

   
    }
        

        

        
