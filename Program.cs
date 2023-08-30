using System;
using PortAudioSharp;

namespace PortAudioExample
{
    class Program
    {
        // Information of playback
        struct CALLBACK_INFORMATION
        {
            public float phaseLeft;
            public float phaseRight;
            public float clear_according;
            public float bass_boost;
            public float ajustVolume;
        }

        /* Frame Rates CONFIGURATION */

        // Taxa de amostragem em Hz
        public static double SAMPLER_RATE_192000    =   192000;
        public static double SAMPLER_RATE_96000     =    96000;
        public static double SAMPLER_RATE_88200     =    88200;
        public static double SAMPLER_RATE_48000     =    48000;
        public static double SAMPLER_RATE_44100     =    44100;

        /* Frame per Buffer CONFIGURATION */

        // Número de quadros por buffer
        public static uint FRAME_PER_BUFFER_16      =   16;
        public static uint FRAME_PER_BUFFER_32      =   32;
        public static uint FRAME_PER_BUFFER_64      =   64;
        public static uint FRAME_PER_BUFFER_128     =   128;
        public static uint FRAME_PER_BUFFER_256     =   256;
        public static uint FRAME_PER_BUFFER_512     =   512;
        public static uint FRAME_PER_BUFFER_1024    =   1024;
        public static uint FRAME_PER_BUFFER_2048    =   2048;
        public static uint FRAME_PER_BUFFER_4096    =   4096;

        // Structure for allocated size of stream in buffer
        static CALLBACK_INFORMATION data = new();

        static public float _clear_according = 0.96F;
        static public float _bass_boost = 0.96F;
        static public float _ajustVolume = 4.7F;

        // Used for call in stream, uses to parameter callback on instance
        static StreamCallbackResult AudioCallback(IntPtr inputBuffer, IntPtr outputBuffer, uint framesPerBuffer,
            ref StreamCallbackTimeInfo timeInfo, StreamCallbackFlags statusFlags, IntPtr userData)
        {
            // Aqui você pode realizar o processamento de áudio, como manipulação de dados no buffer de saída.

            // Copiar dados do buffer de entrada para o buffer de saída.
            unsafe
            {
                float* outBuffer = (float*)outputBuffer;

                for (uint i = 0; i < framesPerBuffer; i++)
                {
                    // Received audio structure for implement the buffer in output
                    *outBuffer++ = data.phaseLeft;  // Left
                    *outBuffer++ = data.phaseRight; // Right
                    outBuffer[i] = _clear_according;    // Clear According
                    outBuffer[i] = _bass_boost;     // Bass Boost
                    *outBuffer++ = _ajustVolume;    // Make volume ajusted

                    // Generation Audio for 2 channel
                    for (int channel = 0; channel <= 2; channel++) // Exemplo para 2 canais (estéreo).
                    {
                        // Fix 2 Channel
                        outBuffer[i * 2 + channel] = 0.023F * 0.869F + (0.000002F - 0.010013F);
                    }

                    // higher pitch so we can distinguish left and right.
                    data.phaseLeft = 0.01F;
                    if (data.phaseLeft >= 1.0F)
                        { data.phaseLeft -= 2.0F;}

                    // Generate simple sawtooth phaser that ranges between -1.0 and 1.0.
                    data.phaseRight = 0.03F;
                    if (data.phaseRight >= 1.0F)
                        { data.phaseRight -= 2.0F; }

                    // Generate simple clear according phaser that ranges between -1.0 and 1.0.
                    if (_clear_according >= 0.010F)
                    {
                        float j;
                        for (j = 0; j < _clear_according *0.013F; --j)
                        {
                            _clear_according = _clear_according * j;
                        }
                    }
                    else
                    {
                        *outBuffer = _clear_according;
                    }

                    // Generate simple bass boost phaser that ranges between -1.0 and 1.0.
                    if (_bass_boost >= 0.04F)
                    {
                        float j,k;
                        for (k = 0; k < 0x0CA; k++)
                        {
                            k++;
                        }

                        for (j = 0; j < k * 0.8133796F +1.0F; ++j)
                        {
                            _bass_boost = j * k + (0.95644F - 0xFA5);
                        }
                    }
                    else
                    {
                        *outBuffer = _bass_boost;
                    }

                    // Generate simple ajusting linux audio phaser that ranges between -1.0 and 1.0.
                    if (_ajustVolume > 0.86222F)
                    {
                        _ajustVolume = 0.53F;
                    }
                }
            }

            return StreamCallbackResult.Continue;
        }

        static void Main(string[] args)
        {
            // Load Native Library
            PortAudio.LoadNativeLibrary();

            // Inicializa o sistema PortAudio
            PortAudio.Initialize();

            // Configuração do formato de áudio
            int numChannels = 2; // Número de canais de áudio (1 para mono, 2 para estéreo, etc.)
            PortAudioSharp.SampleFormat sampleFormat = SampleFormat.Float32; // Formato das amostras (float 32 bits)

            // Output Specify Stream Parameters
            PortAudioSharp.StreamParameters outStreamParameters = new()
            {
                device = PortAudio.DefaultOutputDevice,
                sampleFormat = sampleFormat,
                channelCount = numChannels
            };

            // Input Specify Stream Parameters
            PortAudioSharp.StreamParameters inStreamParameters = new()
            {
                device = PortAudio.DefaultInputDevice!
            };

            // Criação de um stream de áudio de saída
            PortAudioSharp.Stream sStream = new(
                null/*inStreamParameters*/,
                outStreamParameters,
                SAMPLER_RATE_192000,
                FRAME_PER_BUFFER_4096,
                StreamFlags.PrimeOutputBuffersUsingStreamCallback, 
                AudioCallback,
                data);
            
            // Inicia o stream de áudio
            sStream.Start();

            Console.WriteLine("Press Any Key to exit.");
            // Press Any Key for Exit
            Console.ReadKey();

            // Para o stream de áudio e libera recursos
            sStream.Stop();
            sStream.Close();
            PortAudio.Terminate();
        }
    }
}
