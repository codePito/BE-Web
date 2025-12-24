using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Model.Entities;
using WebApp.Model.Request;
using WebApp.Model.Response;

namespace WebApp.Helper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ******************* DTO -> ENTITY *******************

            //Product
            CreateMap<ProductRequest, Product>()
                .ForMember(dest => dest.CategoryID,
                    opt => opt.MapFrom(src => src.CategoryId))
                .ForMember(dest => dest.StockQuantity,
                    opt => opt.MapFrom(src => src.StockQuantity))
                .ForMember(dest => dest.LowStockThreshold,
                    opt => opt.MapFrom(src => src.LowStockThreshold));

            //Images
            CreateMap<Image, ImageResponse>()
                .ForMember(dest => dest.FileSizeFormatted,
                    opt => opt.MapFrom(src => src.FileSizeFormatted))
                .ForMember(dest => dest.Dimensions,
                    opt => opt.MapFrom(src => src.Dimension));

            //Category
            CreateMap<CategoryRequest, Category>();

            //User
            CreateMap<UserRequest, User>();

            //CartItem
            CreateMap<CartItem, CartItemResponse>()
                .ForMember(dest => dest.ProductName,
                    opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.Price,
                    opt => opt.MapFrom(src => src.Product.Price))
                .ForMember(dest => dest.ProductImageUrl,
                    opt => opt.MapFrom(src => src.Product != null ? src.Product.PrimaryImageUrl : null));

            //Cart
            CreateMap<Cart, CartResponse>();
            
            //Image
            CreateMap<Image, ImageResponse>()
                .ForMember(dest => dest.FileSizeFormatted,
                    opt => opt.MapFrom(src => src.FileSizeFormatted))
                .ForMember(dest => dest.Dimensions,
                    opt => opt.MapFrom(src => src.Dimension));

            //******************* Entity -> Response *******************
            //Order
            CreateMap<Order, OrderResponse>()
                .ForMember(o => o.Status, opt => opt.MapFrom(s => s.Status.ToString()))
                .ForMember(o => o.Items, opt => opt.MapFrom(s => s.Items))
                .ForMember(o => o.UserName, opt => opt.MapFrom(s => s.User != null ? s.User.UserName : null))
                .ForMember(o => o.UserEmail, opt => opt.MapFrom(s => s.User != null ? s.User.Email : null));

            CreateMap<OrderItem, OrderItemResponse>()
                .ForMember(d => d.Total,
                opt => opt.MapFrom(s => s.Total));

            CreateMap<Payment, PaymentResponse>()
                .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));

            //Product
            CreateMap<Product, ProductResponse>()
                .ForMember(dest => dest.PrimaryImageUrl,
                    opt => opt.MapFrom(src => src.PrimaryImageUrl))
                .ForMember(dest => dest.ImageUrls,
                    opt => opt.MapFrom(src => src.ImageUrls))
                .ForMember(dest => dest.StockQuantity,
                        opt => opt.MapFrom(src => src.StockQuantity))
                .ForMember(dest => dest.SoldCount,
                        opt => opt.MapFrom(src => src.SoldCount))
                .ForMember(dest => dest.LowStockThreshold,
                        opt => opt.MapFrom(src => src.LowStockThreshold))
                .ForMember(dest => dest.IsAvailable,
                        opt => opt.MapFrom(src => src.IsAvailable))
                .ForMember(dest => dest.IsOutOfStock,
                        opt => opt.MapFrom(src => src.IsOutOfStock))
                .ForMember(dest => dest.IsLowStock,
                        opt => opt.MapFrom(src => src.IsLowStock));


            //Product Image
            //CreateMap<ProductImage, ProductImageResponse>();  

            CreateMap<Category, CategoryResponse>()
                .ForMember(dest => dest.Products,
                    opt => opt.MapFrom(src => src.Products));

            //User
            CreateMap<User, UserResponse>()
                .ForMember(dest => dest.AvatarUrl,
                    opt => opt.MapFrom(src => src.AvatarUrl));

        }
    }
}
