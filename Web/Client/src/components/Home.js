/* eslint-disable react-hooks/exhaustive-deps */
/* eslint-disable default-case */
import React, { useEffect, useMemo, useState } from 'react';

import { kaReducer, Table } from 'ka-table';
import { DataType, EditingMode, SortingMode, SortDirection, PagingPosition, FilteringMode } from 'ka-table/enums';
import { CSVLink } from 'react-csv';
import Autosuggest from 'react-autosuggest';

import * as Cell from './CustomCells';

// TODO: Show expected price from input
// TODO: Full text search
// TODO: Show Details
// TODO: Chain blocks

const tablePropsInit = {
    columns: [
        { key: 'LCSCPart', title: 'LCSC', dataType: DataType.String, width: 25 },
        { key: 'Package', title: 'Package', dataType: DataType.String, width: 35 },
        { key: 'Manufacturer', title: 'Manufacturer', dataType: DataType.String, width: 50 },
        { key: 'LibraryType', title: 'Library Type', dataType: DataType.String, width: 25, sortDirection: SortDirection.Ascend },
        { key: 'Description', title: 'Description', dataType: DataType.String, width: 150 },
        { key: 'Price', title: 'Price', dataType: DataType.Number, width: 30 },
        { key: 'Stock', title: 'Stock', dataType: DataType.Number, width: 30 },
    ],
    format: ({ column, value }) => {
        if (column.key === 'Price')
            return `$${value.toPrecision(3)}`;
    },
    childComponents: {
        cellText: {
            content: (props) => {
                switch (props.column.key) {
                    case 'LCSCPart':
                        return <Cell.LCSCCell Datasheet={props.rowData.Datasheet} {...props} />;
                    case 'LibraryType':
                        return <Cell.LibraryTypeCell {...props} />;
                }
            }
        },
        headFilterButton: {
            content: ({ column: { key } }) => key !== 'LibraryType' && key !== 'Package' && <></>,
        }
    },
    paging: {
        enabled: true,
        pageIndex: 0,
        pageSize: 50,
        pageSizes: [50, 200, 500],
        position: PagingPosition.TopAndBottom
    },
    height: '100%',
    editingMode: EditingMode.None,
    rowKeyField: 'LCSCPart',
    sortingMode: SortingMode.Single,
    filteringMode: FilteringMode.HeaderFilter
};
export function Home(props) {
    const [isLoading, setIsLoading] = useState(false);

    const [inputValue, setInputValue] = useState('');
    const [caretPosition, setCaretPosition] = useState(-1);
    const [suggestions, setSuggestions] = useState([]);

    const [baseProperties, setBaseProperties] = useState([]);
    const [mainCategories, setMainCategories] = useState([]);

    const [selectedMainCategory, setSelectedMainCategory] = useState(null);
    const [extraProperties, setExtraProperties] = useState([]);

    const [clientKeys, setClientKeys] = useState([]);
    const [clientValues, setClientValues] = useState([]);

    // const [queryQueue, setQueryQueue] = useState([]);

    const [tableData, setTableData] = useState([]);
    const [tableProps, changeTableProps] = useState(tablePropsInit);
    const dispatch = action => {
        changeTableProps(prevState => kaReducer(prevState, action));
    };

    useEffect(() => {
        props.socketObserver.on('message', (event) => {
            const response = JSON.parse(event.data);
            if (response.command === 0) {
                setMainCategories(response.result);
                return;
            }
            if (response.command === 1) {
                setBaseProperties(response.result);
                return;
            }
            if (response.command === 2) {
                setExtraProperties(response.result);
                return;
            }

            if (response.command === 3) {
                setTimeout(() => {
                    setTableData(response.result);
                    setIsLoading(false);
                }, 500);
                return;
            }
        });

        GetMainCategories();
        GetBaseProperties();

    }, []);

    const validMainCategory = useMemo(() => selectedMainCategory != null && mainCategories?.includes(selectedMainCategory), [selectedMainCategory, mainCategories]);

    useEffect(() => {
        if (selectedMainCategory != null && selectedMainCategory !== '') {
            GetExtraProperties();
            return;
        }

        // Clear
        // setClientMap(new Map());
        setClientKeys([]);
        setClientValues([]);
    }, [selectedMainCategory]);
    useEffect(() => {
        if (!validMainCategory || extraProperties.length === 0)
            return;

        setClientKeys([]);
        setClientValues([]);
        console.log(extraProperties);

        const newClientKeys = [];
        const newClientValues = [];
        for (const property of extraProperties) {
            const key = property.charAt(0).toUpperCase();
            let keyNum = 1;

            let entry = key + (keyNum.toString());
            while (clientKeys.includes(entry)) {
                keyNum = keyNum + 1;
                entry = key + (keyNum.toString());
            }

            newClientKeys.push(entry);
            newClientValues.push(property);
        }

        setClientKeys(newClientKeys);
        setClientValues(newClientValues);
    }, [extraProperties]);

    function GetMainCategories() {
        props.Send({
            Command: 0
        });
    }
    function GetBaseProperties() {
        props.Send({
            Command: 1
        });
    }
    function GetExtraProperties() {
        if (validMainCategory === false) {
            console.error("Invalid main category");
            return;
        }

        props.Send({
            Command: 2,
            Data: {
                MainCategory: selectedMainCategory,
            }
        })
    }
    function GetResults(query) {
        setTableData([]);
        setIsLoading(true);
        props.Send({
            Command: 3,
            Data: {
                MainCategory: selectedMainCategory,
                ClientMap: Object.fromEntries(clientKeys.map((_, i) => [clientKeys[i], clientValues[i]])),
                Query: query
            }
        })
    }

    // function addQueueItem() {
    //     const value = inputValue;
    //     if (value == null || value == '' || queryQueue.includes(value))
    //         return;

    //     // const parsedValue = clientMap.get(value)
    //     setQueryQueue(old => [...old, value]);
    //     setInputValue('');
    // }
    // function renderQueueItem(item, i) {
    //     const removeItemFn = () => {
    //         const newQueue = [...queryQueue];
    //         newQueue.splice(i, 1);
    //         setQueryQueue(newQueue);
    //     };
    //     return (
    //         <li key={i} className="list-group-item align-items-center d-flex flex-row">
    //             <div>
    //                 {item}
    //             </div>
    //             <button
    //                 className='btn btn-danger py-1 px-2 ms-auto'
    //                 onClick={() => removeItemFn()}
    //             >X</button>
    //         </li>);
    // }

    // Autocomplete
    function onChange(event, { newValue, method }) {


        if (validMainCategory === false) {
            const match = mainCategories.filter(category => category.toLowerCase() === newValue.toLowerCase());

            if (match.length > 0) {
                setInputValue('');
                setSelectedMainCategory(match[0]);
                return;
            }
        }

        setInputValue(newValue);

        // TODO: Autocomplete client values to client keys
    };

    function onKeyUp(event) {
        if (validMainCategory)
            setCaretPosition(event.target.selectionStart);
    };

    function findLastContextWord() {
        let startIndex = 0;
        let endIndex = -1;

        const alphaNumericRegex = /[^0-9a-zA-Z]/;
        for (let i = caretPosition - 1; ; i--) {
            const char = inputValue[i];

            if (alphaNumericRegex.test(char) === true || char === undefined) {
                startIndex = i;
                break;
            }
        }
        for (let i = caretPosition - 1; ; i++) {
            const char = inputValue[i];

            if (alphaNumericRegex.test(char) === true || char === undefined) {
                endIndex = i;
                break;
            }
        }

        return [startIndex, endIndex];
    }

    function onSuggestionsFetchRequested({ value }) {
        setSuggestions(getSuggestions(value));
    }

    function onSuggestionsClearRequested() {
        setSuggestions([]);
    }

    function onSuggestionSelected(event, { suggestion }) {
        if (validMainCategory) {
            if (baseProperties.includes(suggestion)) {
                const [startIndex, endIndex] = findLastContextWord();

                const replacedValue = inputValue.substring(0, startIndex + 1) + suggestion + inputValue.substring(endIndex, inputValue.length);
                setInputValue(replacedValue);
                return;
            }

            const extraPropertyValue = clientValues[clientKeys.indexOf(suggestion)];
            if (extraProperties.includes(extraPropertyValue)) {
                const [startIndex, endIndex] = findLastContextWord();

                const replacedValue = inputValue.substring(0, startIndex + 1) + suggestion + inputValue.substring(endIndex, inputValue.length);
                setInputValue(replacedValue);
                return;
            }

            return;
        }

        setSelectedMainCategory(suggestion);
        setInputValue('');
        setTimeout(() => {
            setSuggestions([]);
        }, 0);
    }

    function renderSuggestion(suggestion) {
        const idx = clientKeys.indexOf(suggestion);
        return (
            <span>{suggestion} {idx >= 0 ? `(${clientValues[idx]})` : null}</span>
        );
    }

    function getSuggestionValue(suggestion) {
        return suggestion;
    }

    function getSuggestions(value) {
        const valueLower = value.toLowerCase();
        if (validMainCategory) {
            const valueSplit = value.toLowerCase().split(' ');
            const suggestionList = baseProperties.concat(clientKeys)
            const suggestions = [];
            for (let i = 0; i < valueSplit.length; i++) {
                for (let k = 0; k < suggestionList.length; k++) {
                    const suggestionWord = suggestionList[k].toLowerCase(); // package
                    if (valueLower.includes(suggestionWord)) // librarytype
                        continue;

                    if (suggestionWord.includes(valueSplit[i]))
                        suggestions.push(suggestionList[k]);
                }
            }
            return suggestions;
        }
        return mainCategories.filter(category => category.toLowerCase().includes(valueLower));
    }

    return (
        <>
            {/* <button onClick={() => setTableData([])}>Hide table</button> */}
            {/* <button onClick={() => setSelectedMainCategory('Capacitors')}>Set main category</button>
            <button onClick={() => setSelectedMainCategory(null)}>Clear main category</button>
            <button onClick={() => setQueryQueue([])}>Clear query queue</button> */}
            <div className='search-bar mt-4'>
                {validMainCategory ? (
                    <div className='main-category-wrapper'>
                        <div className='main-category'>
                            <span className='main-category-text'>
                                {selectedMainCategory}
                            </span>
                        </div>
                        <div className='clear-category-button'>
                            <button type="button"
                                className="btn-close btn-close-white"
                                aria-label="Close"
                                onClick={() => {
                                    setSelectedMainCategory(null);
                                    setInputValue('');
                                }}
                            ></button>
                        </div>
                    </div>
                ) : null}
                <Autosuggest
                    suggestions={suggestions}
                    onSuggestionsFetchRequested={onSuggestionsFetchRequested}
                    onSuggestionsClearRequested={onSuggestionsClearRequested}
                    getSuggestionValue={getSuggestionValue}
                    renderSuggestion={renderSuggestion}
                    onSuggestionSelected={onSuggestionSelected}
                    alwaysRenderSuggestions={true}
                    inputProps={{
                        onChange: onChange,
                        onKeyUp: onKeyUp,
                        value: inputValue,
                        placeholder: validMainCategory ? 'C1 < 200uF and Price < 0.2 and LibraryType = Basic' : 'Enter component category: e.g. Capacitors'
                    }} />
                <div className='button-area'>
                    <button className='btn btn-primary' disabled={!validMainCategory || inputValue === ''} onClick={() => GetResults(inputValue)}>
                        <i className="bi bi-cpu" title='Run Query'></i>
                    </button>
                </div>
            </div>
            {/* <button onClick={() => GetMainCategories()}>GetMainCategories</button>
                <button onClick={() => GetBaseProperties()}>GetBaseProperties</button>
                <button onClick={() => GetExtraProperties()}>GetExtraProperties</button>
                <button onClick={() => GetResults()}>GetResults</button> */}
            {/* <ul className="list-group w-100">
                {queryQueue.map(renderQueueItem)}
            </ul> */}
            {/* {queryQueue.length > 0 ? (<button onClick={() => GetResults()}>Run Queries</button>) : null} */}
            {isLoading ? <div className="spinner-border" role="status">
                <span className="visually-hidden">Loading...</span>
            </div> :
                (tableData.length > 0 ? (<>
                    <div className='d-flex flex-row my-2 gap-2 align-self-end'>
                        <button className='btn btn-sm btn-outline-danger' onClick={() => setTableData([])}>Clear results</button>
                        <CSVLink
                            data={tableData}
                            headers={tableProps.columns.map(c => ({ label: c.title, key: c.key }))}
                            filename='jlcdb.csv'
                            enclosingCharacter={''}
                            separator={';'}
                            className='btn btn-sm btn-outline-dark'>
                            Download results as .csv
                        </CSVLink>
                    </div>
                    <Table {...tableProps}
                        data={tableData}
                        dispatch={dispatch} />
                </>) : <h1 className='display-6 my-auto'>No component found</h1>)
            }

        </>
    );
}