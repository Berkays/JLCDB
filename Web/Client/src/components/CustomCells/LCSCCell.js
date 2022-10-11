export const LCSCCell = ({ Datasheet, column, dispatch, rowKeyValue, value }) => {
    return (
        <span style={{ whiteSpace: 'noWrap' }}>
            <a target='_blank' rel='noopener noreferrer' href={Datasheet}>
                <i className="bi bi-filetype-pdf pe-1" style={{ fontSize: '1.75rem', color: '#F40F02' }} />
            </a>
            <a target='_blank' rel='noopener noreferrer' href={`https://jlcpcb.com/parts/componentSearch?isSearch=true&searchTxt=${value}`}>
                {value}
            </a>
        </span >
    );
};