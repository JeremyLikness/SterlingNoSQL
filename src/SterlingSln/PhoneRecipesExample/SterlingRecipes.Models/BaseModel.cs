namespace SterlingRecipes.Models
{
    /// <summary>
    ///     Base model - we'll use integer keys
    /// </summary>
    public interface IBaseModel
    {
        int Id { get; set; }
    }

    /// <summary>
    ///     Seems recursive but allowed to type this as a model with an identifier
    /// </summary>
    /// <typeparam name="T">The type that derives from base model</typeparam>
    public abstract class BaseModel<T> : IBaseModel where T: BaseModel<T>
    {
        /// <summary>
        ///     Standard key
        /// </summary>
        public int Id { get; set; }       

        /// <summary>
        ///     Make it easy for equality 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj is T && ((T) obj).Id.Equals(Id);
        }

        /// <summary>
        ///     Hash code is the identifier
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Id;
        }

        /// <summary>
        ///     Be helpful for debugging
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0} with Id {1}", typeof (T).FullName, Id);
        }
    }
}