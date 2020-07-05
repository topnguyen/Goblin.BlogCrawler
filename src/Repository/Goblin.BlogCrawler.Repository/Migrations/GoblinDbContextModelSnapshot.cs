﻿// <auto-generated />
using System;
using Goblin.BlogCrawler.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Goblin.BlogCrawler.Repository.Migrations
{
    [DbContext(typeof(GoblinDbContext))]
    partial class GoblinDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Goblin.BlogCrawler.Contract.Repository.Models.PostEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("AuthorAvatarUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("AuthorName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long?>("CreatedBy")
                        .HasColumnType("bigint");

                    b.Property<DateTimeOffset>("CreatedTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<long?>("DeletedBy")
                        .HasColumnType("bigint");

                    b.Property<DateTimeOffset?>("DeletedTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ImageUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset>("LastCrawledTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<long?>("LastUpdatedBy")
                        .HasColumnType("bigint");

                    b.Property<DateTimeOffset>("LastUpdatedTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset>("PublishTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("SiteName")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Tags")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Title")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Url")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("CreatedTime");

                    b.HasIndex("DeletedTime");

                    b.HasIndex("Id");

                    b.HasIndex("LastCrawledTime");

                    b.HasIndex("LastUpdatedTime");

                    b.HasIndex("PublishTime");

                    b.HasIndex("SiteName");

                    b.HasIndex("Title");

                    b.HasIndex("Url");

                    b.ToTable("Post");
                });

            modelBuilder.Entity("Goblin.BlogCrawler.Contract.Repository.Models.SourceEntity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<long?>("CreatedBy")
                        .HasColumnType("bigint");

                    b.Property<DateTimeOffset>("CreatedTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<long?>("DeletedBy")
                        .HasColumnType("bigint");

                    b.Property<DateTimeOffset?>("DeletedTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset>("LastCrawlEndTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset>("LastCrawlStartTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("LastCrawledPostUrl")
                        .HasColumnType("nvarchar(450)");

                    b.Property<long?>("LastUpdatedBy")
                        .HasColumnType("bigint");

                    b.Property<DateTimeOffset>("LastUpdatedTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.Property<TimeSpan>("TimeSpent")
                        .HasColumnType("time");

                    b.Property<long>("TotalPostCrawled")
                        .HasColumnType("bigint");

                    b.Property<long>("TotalPostCrawledLastTime")
                        .HasColumnType("bigint");

                    b.Property<string>("Url")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("CreatedTime");

                    b.HasIndex("DeletedTime");

                    b.HasIndex("Id");

                    b.HasIndex("LastCrawledPostUrl");

                    b.HasIndex("LastUpdatedTime");

                    b.HasIndex("Name");

                    b.HasIndex("Url");

                    b.ToTable("Source");
                });
#pragma warning restore 612, 618
        }
    }
}
