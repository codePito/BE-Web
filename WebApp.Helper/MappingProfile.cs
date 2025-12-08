using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Model;
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
                    opt => opt.MapFrom(src => src.CategoryId));

            //Product Image
            CreateMap<ProductImageRequest, ProductImage>();

            //Category
            CreateMap<CategoryRequest, Category>();

            //User
            CreateMap<UserRequest, User>();

            //CartItem
            CreateMap<CartItem, CartItemResponse>()
                .ForMember(dest => dest.ProductName,
                    opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.Price,
                    opt => opt.MapFrom(src => src.Product.Price));
            //Cart
            CreateMap<Cart, CartResponse>();



            //******************* Entity -> Response *******************
            //Order
            CreateMap<Order, OrderResponse>()
                .ForMember(o => o.Status, opt => opt.MapFrom(s => s.Status.ToString()))
                .ForMember(o => o.Items, opt => opt.MapFrom(s => s.Items));

            CreateMap<OrderItem, OrderItemResponse>()
                .ForMember(d => d.Total,
                opt => opt.MapFrom(s => s.Total));

            CreateMap<Payment, PaymentResponse>()
                .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));

            //Product
            CreateMap<Product, ProductResponse>()
                //.ForMember(dest => dest.CategoryName,
                //    opt => opt.MapFrom(src => src.Category.Name))

                .ForMember(dest => dest.Images,
                    opt => opt.MapFrom(src => src.Images));

            //Product Image
            CreateMap<ProductImage, ProductImageResponse>();  

            CreateMap<Category, CategoryResponse>()
                .ForMember(dest => dest.Products,
                    opt => opt.MapFrom(src => src.Products));

            //User
            CreateMap<User, UserResponse>();


        }
    }
}
