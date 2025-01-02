namespace SongProcessor.Models;

[Flags]
public enum Status : uint
{
	NotSubmitted = 0,
	Submitted = 1U << 0,
	Mp3 = 1U << 1,
	Res480 = 1U << 2,
	Res720 = 1U << 3,
}