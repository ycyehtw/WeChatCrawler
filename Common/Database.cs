using System;
using System.Collections.Generic;
using System.Data.SQLite;
using Dapper;

namespace WeChatCrawler {
  public class Database : IDisposable {
    private const string DataSourceString = "data source=WeChat.db";
    private readonly SQLiteConnection conn;
    public Database() {
      conn = new SQLiteConnection(DataSourceString);
      conn.Open();
    }
    public IList<Post> GetPostsByVendorAfterPost(int vendorId, int postId) {
      var parameters = new DynamicParameters(new { VendorId = vendorId, PostId = postId });
      var posts = conn.Query<Post>("SELECT * FROM Post WHERE VendorId = @VendorId AND Id > @PostId", parameters).AsList<Post>();
      foreach (var post in posts) {
        post.Pictures = GetPicturesByPost(post.Id);
      }
      return posts;
    }
    public IList<int> GetPostIdsAfterDate(DateTime dt) {
      var parameters = new DynamicParameters(new { Date = dt });
      return conn.Query<int>("SELECT Id FROM Post WHERE Date < @Date", parameters).AsList<int>();
    }
    public void DeletePost(int id) {
      using (var trans = conn.BeginTransaction()) {
        var parameters = new DynamicParameters(new { Id = id });
        conn.Execute("DELETE FROM Picture WHERE PostId = @Id", parameters);
        conn.Execute("DELETE FROM Post WHERE Id = @Id", parameters);
        trans.Commit();
      }
    }
    public bool IsExist(Post post) {
      var parameters = new DynamicParameters(new { post.Id });
      var count = conn.QuerySingle<int>("SELECT count(*) FROM Post WHERE Id = @Id", parameters);
      return count > 0;
    }
    public Post GetPost(int id) {
      var parameters = new DynamicParameters(new { Id = id });
      var post = conn.QuerySingleOrDefault<Post>("SELECT * FROM Post WHERE Id = @Id", parameters);
      post.Pictures = GetPicturesByPost(post.Id);
      return post;
    }
    public IList<Post.Picture> GetPicturesByPost(int postId) {
      var parameters = new DynamicParameters(new { Id = postId });
      return conn.Query<Post.Picture>("SELECT * FROM Picture WHERE PostId = @Id", parameters).AsList<Post.Picture>();
    }
    public int GetPostsCountAfter(int id) {
      var parameters = new DynamicParameters(new { Id = id });
      return conn.QuerySingle<int>("SELECT count(*) FROM Post WHERE Id > @Id", parameters);
    }

    public IList<Vendor> GetAllVendors() {
      return conn.Query<Vendor>($"SELECT * FROM Vendor").AsList<Vendor>();
    }
    public IList<Vendor> GetVendors(bool isEnabled) {
      var parameters = new DynamicParameters(new { IsEnabled = isEnabled });
      return conn.Query<Vendor>($"SELECT * FROM Vendor WHERE IsEnabled = @IsEnabled ORDER BY Code", parameters).AsList<Vendor>();
    }

    public IList<Post> GetPostsByVendorAndStatusAndDate(int vendorId, int postStatus, DateTime from, DateTime to) {
      var parameters = new DynamicParameters(new { VendorId = vendorId, Status = postStatus, From = from, To = to });
      var posts = conn.Query<Post>("SELECT * FROM Post WHERE VendorId = @VendorId AND Status = @Status AND Date >= @From AND Date <= @To ORDER BY Date DESC", parameters).AsList<Post>();
      foreach (var post in posts) {
        post.Pictures = GetPicturesByPost(post.Id);
      }
      return posts;
    }
    public bool IsExist(Vendor vendor) {
      var parameters = new DynamicParameters(new { Id = vendor.VendorId });
      var count = conn.QuerySingle<int>("SELECT count(*) FROM Vendor WHERE VendorId = @Id", parameters);
      return count > 0;
    }
    public void Insert(Vendor vendor) {
      conn.Execute("INSERT INTO Vendor (VendorId, WeChatId, Name, Code, IsEnabled) VALUES (@VendorId, @WeChatId, @Name, @Code, @IsEnabled)", vendor);
    }
    public void Insert(Post post) {
      using (var trans = conn.BeginTransaction()) {
        conn.Execute("INSERT INTO Post (Id, VendorId, Date, Content, KrwPrice, TwdPrice, Spec, Color, Material, Status) VALUES (@Id, @VendorId, @Date, @Content, @KrwPrice, @TwdPrice, @Spec, @Color, @Material, @Status)", post, trans);
        if (post.Pictures != null) {
          foreach (var picture in post.Pictures) {
            var parameters = new DynamicParameters(new { PostId = post.Id, picture.Name, picture.IsFirst });
            if (conn.QuerySingle<int>("SELECT count(*) FROM Picture WHERE PostId = @PostId AND Name = @Name", parameters) == 0)
              conn.Execute($"INSERT INTO Picture (PostId, Name, IsFirst) VALUES (@PostId, @Name, @IsFirst)", parameters, trans);
          }
        }
        trans.Commit();
      }
    }
    public Vendor GetVendorByWeChatId(string weChatId) {
      var parameters = new { WeChatId = weChatId };
      return conn.QuerySingleOrDefault<Vendor>($"SELECT * FROM Vendor WHERE WeChatId = @WeChatId", parameters);
    }
    public int GetLastPostIdByVendor(int vendorId) {
      var parameters = new { VendorId = vendorId };
      return conn.QuerySingle<int>("SELECT ifnull(max(Id), 0) FROM Post WHERE VendorId = @VendorId", parameters);
    }
    public int GetLastPostId() {
      return conn.QuerySingle<int>("SELECT ifnull(max(Id), 0) FROM Post");
    }
    public int GetLastVendorId() {
      return conn.QuerySingle<int>("SELECT ifnull(max(VendorId), 0) FROM Vendor");
    }
    public void Update(Post post) {
      using (var trans = conn.BeginTransaction()) {
        conn.Execute("UPDATE Post SET KrwPrice = @KrwPrice, TwdPrice = @TwdPrice, Spec = @Spec, Color = @Color, Material = @Material, Status = @Status WHERE Id = @Id", post, trans);
        foreach (var file in post.Pictures) {
          var parameters = new DynamicParameters(new { post.Id, file.Name, file.IsFirst });
          if (conn.QuerySingle<int>("SELECT count(*) FROM Picture WHERE PostId = @Id AND Name = @Name", parameters) == 0)
            conn.Execute($"INSERT INTO Picture (PostId, Name, IsFirst) VALUES (@Id, @Name, @IsFirst)", parameters, trans);
          else
            conn.Execute($"UPDATE Picture SET IsFirst = @IsFirst WHERE PostId = @Id", parameters, trans);
        }
        trans.Commit();
      }
    }
    public void Update(Vendor vendor) {
      conn.Execute($"UPDATE Vendor SET Name = @Name, Code = @Code, IsEnabled = @IsEnabled WHERE VendorId = @VendorId", vendor);
    }
    public void DisableAllVendors() {
      conn.Execute($"UPDATE Vendor SET IsEnabled = 0");
    }
    public void Insert(ExportedPost exportedPost) {
      conn.Execute("INSERT INTO ExportedPost (PostId, PostedDate, ExportedDate) VALUES (@PostId, @PostedDate, @ExportedDate)", exportedPost);
    }
    public bool IsExist(ExportedPost exportedPost) {
      var count = conn.QuerySingle<int>("SELECT count(*) FROM ExportedPost WHERE PostId = @PostId AND PostedDate = @PostedDate", exportedPost);
      return count > 0;
    }
    public User GetUser(string username) {
      var parameters = new DynamicParameters(new { Username = username });
      return conn.QuerySingleOrDefault<User>($"SELECT * FROM User WHERE Username = @Username", parameters);
    }
    public int GetPostCountByVendor(Vendor vendor) {
      return conn.QuerySingle<int>("SELECT count(*) FROM Post WHERE VendorId = @VendorId", vendor);
    }
    public void Dispose() {
      conn?.Dispose();
    }
  }
}
