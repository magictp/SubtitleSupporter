using SubtitleSupporter.model;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Binding;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubtitleSupporter
{
    internal class ParameterBinder : BinderBase<ParameterModel>
    {
        private readonly Option<string> _fileOption;
        private readonly Option<string> _modelOption;
        private readonly Option<string> _coorOption;
        private readonly Option<double> _confidenceOption;
        private readonly Option<string> _outputFilePathOption;
        private readonly Option<string> _subtitleOption;
        private readonly Option<bool> _qsvAccelOption;
        private readonly Option<bool> _localOCROption;
        private readonly Option<int> _startTimeption;
        private readonly Option<int> _endTimeption;


        public ParameterBinder(Option<string> fileOption, Option<string> modelOption, Option<string> coorOption, Option<double> confidenceOption, Option<string> outputFilePathOption, Option<string> subtitleOption, Option<bool> qsvAccelOption, Option<bool> localOCROption, Option<int> startTimeption, Option<int> endTimeption)
        {
            _fileOption = fileOption;
            _modelOption = modelOption;
            _coorOption = coorOption;
            _confidenceOption = confidenceOption;
            _outputFilePathOption = outputFilePathOption;
            _subtitleOption = subtitleOption;
            _qsvAccelOption = qsvAccelOption;
            _localOCROption = localOCROption;
            _startTimeption = startTimeption;
            _endTimeption = endTimeption;
        }

        protected override ParameterModel GetBoundValue(BindingContext bindingContext) =>
            new ParameterModel
            {
                file = bindingContext.ParseResult.GetValueForOption(_fileOption),
                model = bindingContext.ParseResult.GetValueForOption(_modelOption),
                coor = bindingContext.ParseResult.GetValueForOption(_coorOption),
                confidence = bindingContext.ParseResult.GetValueForOption(_confidenceOption),
                output = bindingContext.ParseResult.GetValueForOption(_outputFilePathOption),
                subtitle = bindingContext.ParseResult.GetValueForOption(_subtitleOption),
                qsvAccel = bindingContext.ParseResult.GetValueForOption(_qsvAccelOption),
                localOCR = bindingContext.ParseResult.GetValueForOption(_localOCROption),
                startTime = bindingContext.ParseResult.GetValueForOption(_startTimeption),
                endTime = bindingContext.ParseResult.GetValueForOption(_endTimeption)
            };
    }
}
