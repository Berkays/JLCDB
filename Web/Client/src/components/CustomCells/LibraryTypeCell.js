export const LibraryTypeCell = ({ column, dispatch, rowKeyValue, value }) => {
    return (
        <span style={value === 'Basic' ? { color: '#0f8a07' } : { color: '#CC5500' }}>
            {value}
        </span>
    );
};