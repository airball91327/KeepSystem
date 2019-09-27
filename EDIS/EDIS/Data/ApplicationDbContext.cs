using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EDIS.Models;
using EDIS.Models.RepairModels;
using EDIS.Models.Identity;
using EDIS.Models.LocationModels;

namespace EDIS.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            
        }

        public DbSet<DocIdStore> DocIdStores { get; set; }
        public DbSet<RepairModel> Repairs { get; set; }
        public DbSet<RepairDtlModel> RepairDtls { get; set; }
        public DbSet<RepairFlowModel> RepairFlows { get; set; }
        public DbSet<AppUserModel> AppUsers { get; set; }
        public DbSet<AppRoleModel> AppRoles { get; set; }
        public DbSet<DepartmentModel> Departments { get; set; }
        public DbSet<UsersInRolesModel> UsersInRoles { get; set; }
        public DbSet<BuildingModel> Buildings { get; set; }
        public DbSet<FloorModel> Floors { get; set; }
        public DbSet<PlaceModel> Places { get; set; }
        public DbSet<AssetModel> Assets { get; set; }
        public DbSet<DealStatusModel> DealStatuses { get; set; }
        public DbSet<FailFactorModel> FailFactors { get; set; }
        public DbSet<RepairEmpModel> RepairEmps { get; set; }
        public DbSet<RepairCostModel> RepairCosts { get; set; }
        public DbSet<TicketModel> Tickets { get; set; }
        public DbSet<TicketDtlModel> TicketDtls { get; set; }
        public DbSet<Ticket_seq_tmpModel> Ticket_seq_tmps { get; set; }
        public DbSet<AttainFileModel> AttainFiles { get; set; }
        public DbSet<DeptStockModel> DeptStocks { get; set; }
        public DbSet<DeptStockClassModel> DeptStockClasses { get; set; }
        public DbSet<DeptStockItemModel> DeptStockItems { get; set; }
        public DbSet<VendorModel> Vendors { get; set; }
        public DbSet<FloorEngModel> FloorEngs { get; set; }
        public DbSet<ExternalUserModel> ExternalUsers { get; set; }
        public DbSet<EngsInDeptsModel> EngsInDepts { get; set; }
        public DbSet<EngDealingDocs> EngDealingDocs { get; set; }
        public DbSet<EngSubStaff> EngSubStaff { get; set; }
        public DbSet<ScrapAssetModel> ScrapAssets { get; set; }
        public DbSet<RepairShutRecordsModel> RepairShutRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<AppUserModel>().HasKey(c => c.Id);
            builder.Entity<AppRoleModel>().HasKey(c => c.RoleId);
            builder.Entity<UsersInRolesModel>().HasKey(c => new {c.UserId, c.RoleId});
            builder.Entity<RepairFlowModel>().HasKey(c => new { c.DocId, c.StepId });
            builder.Entity<DocIdStore>().HasKey(c => new { c.DocType, c.DocId});
            builder.Entity<FloorModel>().HasKey(c => new { c.BuildingId, c.FloorId });
            builder.Entity<PlaceModel>().HasKey(c => new { c.BuildingId, c.FloorId, c.PlaceId });
            builder.Entity<RepairEmpModel>().HasKey(c => new { c.DocId, c.UserId });
            builder.Entity<RepairCostModel>().HasKey(c => new { c.DocId, c.SeqNo });
            builder.Entity<TicketDtlModel>().HasKey(c => new { c.TicketDtlNo, c.SeqNo });
            builder.Entity<TicketModel>().HasKey(c => c.TicketNo);
            builder.Entity<Ticket_seq_tmpModel>().HasKey(c => c.YYYMM);
            builder.Entity<AttainFileModel>().HasKey(c => new { c.DocType, c.DocId, c.SeqNo });
            builder.Entity<DeptStockModel>().HasKey(c => new { c.StockId });
            builder.Entity<DeptStockClassModel>().HasKey(c => new { c.StockClsId });
            builder.Entity<DeptStockItemModel>().HasKey(c => new { c.StockClsId, c.StockItemId });
            builder.Entity<VendorModel>().HasKey(c => new { c.VendorId });
            builder.Entity<FloorEngModel>().HasKey(c => new { c.EngId, c.BuildingId, c.FloorId });
            builder.Entity<ExternalUserModel>().HasKey(c => new { c.Id });
            builder.Entity<EngsInDeptsModel>().HasKey(c => new { c.EngId, c.BuildingId, c.FloorId, c.PlaceId });
            builder.Entity<EngDealingDocs>().HasKey(c => new { c.EngId });
            builder.Entity<EngSubStaff>().HasKey(c => new { c.EngId });
            builder.Entity<ScrapAssetModel>().HasKey(c => new { c.DocId, c.DeviceNo, c.AssetNo });
            builder.Entity<RepairShutRecordsModel>().HasKey(c => new { c.DocId });
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
    }
}
