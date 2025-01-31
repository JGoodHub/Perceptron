using System;

namespace NeuralNet
{
    public enum ActivationFunctionType
    {
        Linear,
        Sigmoid,
        ReLU,
        LeakyReLU,
        Tanh
    }

    public static class ActivationFunctions
    {
        /// <summary>
        /// Applies the Linear activation function to the input. <br/>
        /// Simply maps the input directly to the output
        /// </summary>
        public static float Linear(float input)
        {
            return input;
        }

        /// <summary>
        /// Applies the Sigmoid activation function to the input. <br/>
        /// Maps the input to a value between 0 and 1, where large positive inputs approach 1 
        /// and large negative inputs approach 0.
        /// </summary>
        public static float Sigmoid(float input)
        {
            return (1f / (1f + (float) Math.Exp(-input)));
        }

        /// <summary>
        /// Applies the Rectified Linear Unit (ReLU) activation function to the input. <br/>
        /// Returns 0 if the input is less than or equal to 0, otherwise returns the input itself.
        /// </summary>
        public static float ReLU(float input)
        {
            return input <= 0f ? 0f : input;
        }

        /// <summary>
        /// Applies the Leaky Rectified Linear Unit (LeakyReLU) activation function to the input. <br/>
        /// Returns a small fraction (default: 15%) of the input for values less than or equal to 0, 
        /// and the input itself for positive values.
        /// </summary>
        public static float LeakyReLU(float input)
        {
            return input <= 0f ? input * 0.15f : input;
        }

        /// <summary>
        /// Applies the hyperbolic tangent (tanh) activation function to the input. <br/>
        /// Maps the input to a value between -1 and 1, where large positive inputs approach 1 
        /// and large negative inputs approach -1.
        /// </summary>
        public static float Tanh(float input)
        {
            return (float) Math.Tanh(input);
        }

        public static Func<float, float> ForType(ActivationFunctionType activationFunctionType)
        {
            return activationFunctionType switch
            {
                ActivationFunctionType.Linear => Linear,
                ActivationFunctionType.Sigmoid => Sigmoid,
                ActivationFunctionType.ReLU => ReLU,
                ActivationFunctionType.LeakyReLU => LeakyReLU,
                ActivationFunctionType.Tanh => Tanh,
                _ => throw new ArgumentException("Unsupported activation function type.")
            };
        }
    }
}