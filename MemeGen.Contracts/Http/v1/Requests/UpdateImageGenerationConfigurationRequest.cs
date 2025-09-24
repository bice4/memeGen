namespace MemeGen.Contracts.Http.v1.Requests;

public class UpdateImageGenerationConfigurationRequest(int textPadding, bool textAtTop, int backgroundOpacity, bool useUpperText)
{
    public int TextPadding { get; set; } = textPadding;

    public bool TextAtTop { get; set; } = textAtTop;

    public int BackgroundOpacity { get; set; } = backgroundOpacity;
    
    public bool UseUpperText { get; set; } = useUpperText;
}