using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using Microsoft.DirectX;
using Microsoft.DirectX.DirectSound;

namespace StepDX
{
    public class GameSounds
    {
        private Device SoundDevice = null;

        private SecondaryBuffer[] clank = new SecondaryBuffer[10];
        int clankToUse = 0;

        private SecondaryBuffer explode = null;
        private SecondaryBuffer thrust = null;

        public GameSounds(Form form)
        {
            SoundDevice = new Device();
            SoundDevice.SetCooperativeLevel(form, CooperativeLevel.Priority);

            Load(ref explode, "../../Explosion.wav");
            Load(ref thrust, "../../Rocket Thrusters.wav");

            /*for (int i = 0; i < clank.Length; i++)
                Load(ref clank[i], "../../clank.wav");*/
        }

        private void Load(ref SecondaryBuffer buffer, string filename)
        {
            try
            {
                buffer = new SecondaryBuffer(filename, SoundDevice);
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to load " + filename,
                                "Danger, Will Robinson", MessageBoxButtons.OK);
                buffer = null;
            }
        }

        /*public void Clank()
        {
            clankToUse = (clankToUse + 1) % clank.Length;

            if (clank[clankToUse] == null)
                return;

            if (!clank[clankToUse].Status.Playing)
                clank[clankToUse].Play(0, BufferPlayFlags.Default);
        }*/

        public void Explode()
        {
            if (explode == null)
                return;

            if (!explode.Status.Playing)
            {
                if (thrust.Status.Playing)
                    thrust.Stop();
                explode.Play(0, BufferPlayFlags.Default);
            }
        }

        public void Thruster()
        {
            if (thrust == null)
                return;

            if (!thrust.Status.Playing)
            {
                thrust.SetCurrentPosition(0);
                thrust.Play(0, BufferPlayFlags.Default);
            }
        }

        /*public void ArmsEnd()
        {
            if (arms == null)
                return;

            if (arms.Status.Playing)
                arms.Stop();
        }*/

    }
}
