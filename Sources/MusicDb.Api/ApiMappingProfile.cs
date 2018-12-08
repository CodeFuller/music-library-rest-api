using AutoMapper;
using MusicDb.Abstractions.Models;
using MusicDb.Api.Dto.ArtistDto;
using MusicDb.Api.Dto.DiscDto;
using MusicDb.Api.Dto.SongDto;

namespace MusicDb.Api
{
	public class ApiMappingProfile : Profile
	{
		public ApiMappingProfile()
		{
			CreateMap<InputArtistData, Artist>()
				.ForMember(x => x.Id, opt => opt.Ignore())
				.ForMember(x => x.Discs, opt => opt.Ignore());
			CreateMap<Artist, OutputArtistData>()
				.ForMember(x => x.Links, opt => opt.Ignore());

			CreateMap<InputDiscData, Disc>()
				.ForMember(x => x.Id, opt => opt.Ignore())
				.ForMember(x => x.Artist, opt => opt.Ignore())
				.ForMember(x => x.Songs, opt => opt.Ignore());
			CreateMap<Disc, OutputDiscData>()
				.ForMember(x => x.Links, opt => opt.Ignore());

			CreateMap<InputSongData, Song>()
				.ForMember(x => x.Id, opt => opt.Ignore())
				.ForMember(x => x.Disc, opt => opt.Ignore());
			CreateMap<Song, OutputSongData>()
				.ForMember(x => x.Links, opt => opt.Ignore());
		}
	}
}
