using Microsoft.ML.Data;

namespace VPJCountDetection
{
    public class VpjPredictions
    {

        [ColumnName("model_outputs0")]
        public float[] PredictedLabels { get; set; }
    }
}